import assert from 'node:assert/strict';
import { DatabaseSync, backup } from 'node:sqlite';
import { spawn } from 'node:child_process';
import { mkdirSync, appendFileSync, existsSync } from 'node:fs';
import { resolve, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const root = resolve(dirname(fileURLToPath(import.meta.url)), '../..');
const apiDll = resolve(root, 'mobile/EzCondo.Mobile.Api/bin/Debug/net9.0/EzCondo.Mobile.Api.dll');
assert(existsSync(apiDll), 'Build the mobile API first.');
const local = resolve(root, 'mobile/.local');
mkdirSync(local, { recursive: true });
const testDb = resolve(local, `smoke-${Date.now()}.db`);
const source = new DatabaseSync(resolve(root, 'EzCondo.Web/backend/ezcondo.db'), { readOnly: true });
await backup(source, testDb);
source.close();
const fixture = new DatabaseSync(testDb);
// Mirror the legacy shape, including a second tenant and a second household.
fixture.exec(`
INSERT INTO UserAccounts SELECT 'test-neighbor','Test Neighbor','0900000022','neighbor@test.local','resident','B-0802',NULL,'2026-01-01','tenant-1';
INSERT INTO Apartments SELECT 'test-apartment','TEST-200','T',2,65,'2PN','occupied',2,1,'Đông','Cơ bản',900000,NULL,NULL,'[]',NULL,'Test Tenant Two',NULL,NULL,1,'tenant-2';
INSERT INTO UserAccounts SELECT 'test-other','Test Tenant Two','0900000033','other@test.local','resident','TEST-200',NULL,'2026-01-01','tenant-2';
INSERT INTO Complaints (Id,TicketCode,Title,Category,ApartmentCode,ResidentName,ResidentPhone,Priority,Status,CreatedAt,UpdatedAt,Description,Images)
VALUES ('test-other-complaint','TEST-200','Private other tenant','Khác','TEST-200','Test Tenant Two','','medium','new','2026-11-12','2026-11-12','Must stay private','[]');
`);
fixture.close();
const base = 'http://127.0.0.1:5275';
try {
    await fetch(base + '/health', { signal: AbortSignal.timeout(1000) });
    throw new Error('Port 5275 is occupied. Stop the local mobile API before running this isolated test.');
} catch (error) {
    if (error.message.includes('occupied')) throw error;
}
const api = spawn('dotnet', [apiDll], {
    cwd: resolve(root, 'mobile/EzCondo.Mobile.Api'),
    env: { ...process.env, ASPNETCORE_ENVIRONMENT: 'Development', MobileDatabase: testDb },
    stdio: ['ignore', 'pipe', 'pipe'], windowsHide: true
});
api.stdout.on('data', data => appendFileSync(resolve(local, 'smoke-api.log'), data));
api.stderr.on('data', data => appendFileSync(resolve(local, 'smoke-api.log'), data));
let checks = 0;
async function request(path, { token, method = 'GET', body, status = 200 } = {}) {
    const response = await fetch(base + '/api/mobile/' + path, {
        method, headers: { ...(token ? { Authorization: 'Bearer ' + token } : {}), ...(body ? { 'Content-Type': 'application/json' } : {}) },
        body: body ? JSON.stringify(body) : undefined, signal: AbortSignal.timeout(15000)
    });
    const text = await response.text();
    assert.equal(response.status, status, `${method} ${path}: ${text}`);
    checks++;
    return text ? JSON.parse(text) : null;
}
async function login(id) { return (await request('sessions', { method: 'POST', body: { accountId: id } })).token; }
try {
    let ready = false;
    for (let attempt = 0; attempt < 40; attempt++) {
        if (api.exitCode !== null) throw new Error('API exited, inspect mobile/.local/smoke-api.log');
        try { ready = (await fetch(base + '/health')).ok; } catch { }
        if (ready) break;
        await new Promise(resolve => setTimeout(resolve, 500));
    }
    assert(ready, 'API readiness');
    await request('profile', { status: 401 });
    await request('profile', { token: 'invalid', status: 401 });
    await request('sessions', { method: 'POST', body: { accountId: 'usr-02' }, status: 401 });
    const resident = await login('usr-01');
    const staff = await login('usr-03');
    const neighbor = await login('test-neighbor');
    const other = await login('test-other');
    const accounts = await request('demo-accounts');
    assert(accounts.every(a => ['resident', 'staff'].includes(a.role)));
    const profile = await request('profile', { token: resident });
    assert.equal(profile.account.apartmentCode, 'A-1204');
    const invoices = await request('invoices', { token: resident });
    assert(invoices.length > 0 && invoices.every(i => i.items.length > 0));
    for (const path of ['overview', 'contracts', 'parcels', 'bookings', 'services']) await request(path, { token: resident });
    await request('shifts', { token: resident, status: 403 });
    await request('invoices', { token: staff, status: 403 });
    const shifts = await request('shifts', { token: staff });
    assert.equal(shifts.length, 1);
    assert.equal((await request('profile', { token: staff })).department, 'Kỹ thuật');
    const assigned = await request('complaints', { token: staff });
    assert(assigned.length > 0 && assigned.every(c => c.assignedTo === 'KTV Phạm Văn Tuấn' || c.assignedTo === 'Phạm Văn Tuấn'));
    await request('complaints/cmp-02', { token: resident, status: 404 });
    await request('complaints/cmp-02', { token: staff, status: 404 });
    await request('complaints/test-other-complaint', { token: resident, status: 404 });
    assert.equal((await request('complaints', { token: other })).length, 1);
    await request('complaints', { token: staff, method: 'POST', body: {}, status: 403 });
    await request('complaints', { token: resident, method: 'POST', body: { title: 'a', category: 'Khác', priority: 'high', description: 'bad' }, status: 400 });
    const complaint = await request('complaints', { token: resident, method: 'POST', status: 201,
        body: { title: 'Kiểm thử đường ống nước', description: 'Nước chảy yếu trong căn hộ vào buổi tối.', category: 'Kỹ thuật - Nước', priority: 'high', apartmentCode: 'TEST-200', residentName: 'Spoof' } });
    assert.equal(complaint.apartmentCode, 'A-1204');
    await request('complaints/' + complaint.id, { token: neighbor, status: 404 });
    await request('complaints/' + complaint.id, { token: other, status: 404 });
    await request('complaints/' + complaint.id + '/messages', { token: resident, method: 'POST', body: { content: '   ' }, status: 400 });
    const message = await request('complaints/' + complaint.id + '/messages', { token: resident, method: 'POST', body: { content: 'Thông tin bổ sung từ cư dân.', senderName: 'Spoof' } });
    assert.equal(message.senderName, 'Nguyễn Văn Minh');
    const saved = await request('complaints/' + complaint.id, { token: resident });
    assert.equal(saved.messages.length, 1);
    await request('complaints/cmp-01/status', { token: resident, method: 'PATCH', body: { expectedStatus: 'in_progress', status: 'resolved' }, status: 403 });
    await request('complaints/cmp-01/status', { token: staff, method: 'PATCH', body: { expectedStatus: 'in_progress', status: 'closed' }, status: 400 });
    await request('complaints/cmp-01/status', { token: staff, method: 'PATCH', body: { expectedStatus: 'in_progress', status: 'waiting_resident' } });
    await request('complaints/cmp-01/status', { token: staff, method: 'PATCH', body: { expectedStatus: 'in_progress', status: 'resolved' }, status: 409 });
    await request('complaints/cmp-01/status', { token: staff, method: 'PATCH', body: { expectedStatus: 'waiting_resident', status: 'resolved' } });
    const date = new Intl.DateTimeFormat('en-CA', { timeZone: 'Asia/Ho_Chi_Minh', year: 'numeric', month: '2-digit', day: '2-digit' }).format(new Date());
    const booking = { facilityName: 'Sân Tennis', date, timeSlot: '08:00 - 10:00', guests: 2 };
    await request('bookings', { token: resident, method: 'POST', body: { ...booking, guests: 0 }, status: 400 });
    await request('bookings', { token: resident, method: 'POST', body: { ...booking, date: '2001-01-01' }, status: 400 });
    const booked = await request('bookings', { token: resident, method: 'POST', body: booking, status: 201 });
    assert.equal(booked.status, 'Chờ duyệt');
    await request('bookings', { token: resident, method: 'POST', body: booking, status: 409 });
    const service = await request('services', { token: resident, method: 'POST', status: 201,
        body: { type: 'Sửa chữa điện nước', date, details: 'Kiểm tra vòi nước trong căn hộ vào buổi chiều.' } });
    assert((await request('services', { token: resident })).some(s => s.id === service.id));
    const tampered = resident.slice(0, -10) + 'AAAAAAAAAA';
    await request('profile', { token: tampered, status: 401 });
    // Fail closed when a code exists in more than one tenant.
    const modify = new DatabaseSync(testDb);
    modify.exec("INSERT INTO Apartments SELECT 'ambiguous-apartment','A-1204','T',2,65,'2PN','occupied',2,1,'Đông','Cơ bản',900000,NULL,NULL,'[]',NULL,'Other',NULL,NULL,1,'tenant-2'");
    modify.close();
    await request('profile', { token: resident, status: 401 });
    assert(!(await request('complaints', { token: staff })).some(c => c.apartmentCode === 'A-1204'));
    const verify = new DatabaseSync(testDb, { readOnly: true });
    assert.equal(verify.prepare('PRAGMA quick_check').get().quick_check, 'ok');
    verify.close();
    console.log(`PASS ${checks} HTTP assertions plus role, ownership, tenant isolation, writes, concurrency and SQLite checks.`);
} finally {
    api.kill();
    await new Promise(resolve => api.once('exit', resolve));
}
