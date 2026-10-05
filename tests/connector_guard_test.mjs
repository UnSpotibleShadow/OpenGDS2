import assert from 'node:assert/strict';
import {createConnectorGuard} from '../tools/connector_guard.mjs';

let passed = 0;
async function test(name, call) {await call(); passed++; console.log(`PASS ${name}`);}
const never = () => new Promise(() => {});
function fixture(options = {}) {
    const records = [];
    const guard = createConnectorGuard({timeoutMs: 15, checkpoint: state => records.push(state), ...options});
    return {guard, records};
}
await test('successful writes require read-back before clearing the checkpoint', async () => {
    const {guard, records} = fixture();
    const result = await guard.run({label: 'update main', mutation: true, expected: 'target', call: () => 'ok', verify: () => true});
    assert.equal(result.status, 'verified');
    assert.equal(guard.unresolvedWrite, null);
    assert.deepEqual(records.map(x => x.status), ['started', 'verified']);
});
await test('timed-out writes block replay until the target is observed', async () => {
    const {guard, records} = fixture();
    const result = await guard.run({label: 'update main', mutation: true, expected: 'target', call: never, verify: () => false});
    assert.equal(result.status, 'unknown');
    assert.equal(records.at(-1).status, 'unknown');
    let calls = 0;
    await assert.rejects(guard.run({label: 'retry', mutation: true, call: () => calls++, verify: () => true}), /Unresolved write/);
    assert.equal(calls, 0);
    assert.equal((await guard.reconcile(() => false)).status, 'unknown');
    assert.ok(guard.unresolvedWrite);
    assert.equal((await guard.reconcile(() => true)).status, 'verified');
    assert.equal(guard.unresolvedWrite, null);
});
await test('late connector completion cannot turn an unknown write into success', async () => {
    const {guard, records} = fixture();
    let finish;
    const waiting = new Promise(resolve => {finish = resolve;});
    const result = await guard.run({label: 'late write', mutation: true, call: () => waiting, verify: () => true});
    assert.equal(result.status, 'unknown');
    finish('completed remotely');
    await Promise.resolve();
    assert.equal(records.at(-1).status, 'unknown');
    assert.ok(guard.unresolvedWrite);
});
await test('verification timeouts and connector errors remain unresolved', async () => {
    for (const call of [() => 'ok', () => {throw new Error('private failure');}, () => ({isError: true})]) {
        const {guard, records} = fixture();
        assert.equal((await guard.run({label: 'write', mutation: true, call, verify: never})).status, 'unknown');
        assert.ok(guard.unresolvedWrite);
        assert.equal(JSON.stringify(records).includes('private failure'), false);
    }
});
await test('reads time out without locking later reads or writes', async () => {
    const {guard} = fixture();
    assert.equal((await guard.run({label: 'read', call: never})).status, 'timed_out');
    assert.equal(guard.unresolvedWrite, null);
    assert.equal((await guard.run({label: 'read again', call: () => 'ok'})).status, 'completed');
});
await test('no mutation starts when its durable checkpoint is unavailable', async () => {
    let calls = 0;
    const {guard} = fixture({checkpoint: never});
    await assert.rejects(guard.run({label: 'write', mutation: true, call: () => calls++, verify: () => true}), /checkpoint unavailable/);
    assert.equal(calls, 0);
});
await test('restored unknown writes and concurrent operations cannot be overwritten', async () => {
    const pendingWrite = {label: 'interrupted main update', mutation: true, expected: 'target', status: 'started'};
    const {guard} = fixture({pendingWrite});
    await assert.rejects(guard.run({label: 'new write', mutation: true, call: () => 'ok', verify: () => true}), /Unresolved write/);
    assert.equal((await guard.reconcile(() => true)).status, 'verified');
    const slow = guard.run({label: 'read', call: never});
    await assert.rejects(guard.run({label: 'other', call: () => 'ok'}), /still active/);
    await slow;
});
await test('invalid deadlines and missing write verifiers are rejected', async () => {
    for (const timeoutMs of [0, -1, NaN, Infinity, 45001]) assert.throws(() => fixture({timeoutMs}), TypeError);
    const {guard} = fixture();
    await assert.rejects(guard.run({label: 'write', mutation: true, call: () => 'ok'}), TypeError);
});
console.log(`${passed} connector guard tests passed`);
