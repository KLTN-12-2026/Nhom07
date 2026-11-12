// SQLite backup includes committed WAL data, even while the web app has the source open.
const { DatabaseSync, backup } = require('node:sqlite');
const { resolve, dirname } = require('node:path');
const { existsSync, mkdirSync } = require('node:fs');

async function main() {
    const source = resolve(__dirname, '../EzCondo.Web/backend/ezcondo.db');
    const target = resolve(process.argv[2] || resolve(__dirname, '.local/mobile.db'));
    if (existsSync(target)) {
        console.log('Using existing local database ' + target);
        return;
    }
    mkdirSync(dirname(target), { recursive: true });
    const db = new DatabaseSync(source, { readOnly: true });
    try {
        await backup(db, target);
        console.log('Created local SQLite backup ' + target);
    } finally {
        db.close();
    }
}
main().catch(error => { console.error(error.message); process.exitCode = 1; });
