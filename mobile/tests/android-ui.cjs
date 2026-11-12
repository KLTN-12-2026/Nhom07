const { execFileSync } = require('node:child_process');
const { writeFileSync, mkdirSync } = require('node:fs');
const { resolve } = require('node:path');
const assert = require('node:assert/strict');
const adb = resolve(process.env['ProgramFiles(x86)'] || 'C:/Program Files (x86)', 'Android/android-sdk/platform-tools/adb.exe');
const device = process.env.ANDROID_SERIAL || 'emulator-5554';
function run(...args) { return execFileSync(adb, ['-s', device, ...args], { encoding: 'utf8', timeout: 20000 }); }
function nodes() {
    let dumped = false;
    for (let attempt = 0; attempt < 4; attempt++) {
        if (run('shell', 'uiautomator', 'dump', '/sdcard/ezcondo-ui.xml').includes('dumped to')) { dumped = true; break; }
        Atomics.wait(new Int32Array(new SharedArrayBuffer(4)), 0, 0, 500);
    }
    assert(dumped, 'Android UI hierarchy unavailable.');
    const xml = run('shell', 'cat', '/sdcard/ezcondo-ui.xml');
    return [...xml.matchAll(/<node\s+([^>]+)>?/g)].map(match => Object.fromEntries(
        [...match[1].matchAll(/([\w-]+)="([^"]*)"/g)].map(a => [a[1], a[2].replaceAll('&amp;', '&').replaceAll('&#10;', '\n').replaceAll('&quot;', '"')])));
}
const [command = 'snapshot', target, value] = process.argv.slice(2);
if (command === 'snapshot') console.log(JSON.stringify(nodes().filter(n => n.text || n['content-desc']).map(n => ({ text: n.text, desc: n['content-desc'], class: n.class, bounds: n.bounds })), null, 2));
else if (command === 'tap' || command === 'input') {
    const matches = n => n.text === target || n['content-desc'] === target;
    let node;
    for (let attempt = 0; attempt < 5; attempt++) {
        const visible = nodes();
        node = visible.find(n => matches(n) && n.class === 'android.widget.Button')
            || visible.find(n => matches(n) && n.class === 'android.widget.EditText')
            || visible.find(n => n.text === target) || visible.find(matches);
        if (node) break;
        Atomics.wait(new Int32Array(new SharedArrayBuffer(4)), 0, 0, 400);
    }
    assert(node, 'Visible Android node not found: ' + target);
    const bounds = node.bounds.match(/\d+/g).map(Number);
    run('shell', 'input', 'tap', String(Math.round((bounds[0] + bounds[2]) / 2)), String(Math.round((bounds[1] + bounds[3]) / 2)));
    if (command === 'input') run('shell', 'input', 'text', value.replaceAll(' ', '%s'));
    console.log(command + ' ' + target);
} else if (command === 'back') run('shell', 'input', 'keyevent', '4');
else if (command === 'down') run('shell', 'input', 'swipe', '540', '1850', '540', '700', '450');
else if (command === 'up') run('shell', 'input', 'swipe', '540', '700', '540', '1850', '450');
else if (command === 'shot') {
    const output = resolve(__dirname, '../.local', (target || 'android') + '.png');
    mkdirSync(resolve(__dirname, '../.local'), { recursive: true });
    writeFileSync(output, execFileSync(adb, ['-s', device, 'exec-out', 'screencap', '-p'], { timeout: 20000 }));
    console.log(output);
} else throw new Error('Unknown UI command ' + command);
