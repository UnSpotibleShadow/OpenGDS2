// Dependency-free guard for connector orchestration. A deadline stops waiting;
// it does not prove that a remote request was cancelled or failed to take effect.
export function createConnectorGuard({checkpoint, timeoutMs = 20000, pendingWrite = null}) {
    if (typeof checkpoint !== "function" || !Number.isFinite(timeoutMs) || timeoutMs <= 0 || timeoutMs > 45000)
        throw new TypeError("Provide a durable checkpoint callback and a deadline of at most 45 seconds.");
    if (pendingWrite && (pendingWrite.mutation !== true || !pendingWrite.label))
        throw new TypeError("Invalid restored write checkpoint.");
    let unresolvedWrite = pendingWrite;
    let active = false;
    async function bounded(call) {
        let timer;
        try {
            return await Promise.race([
                Promise.resolve().then(call).then(value => ({status: "completed", value}),
                    () => ({status: "failed"})),
                new Promise(resolve => {timer = setTimeout(() => resolve({status: "timed_out"}), timeoutMs);})
            ]);
        } finally {clearTimeout(timer);}
    }
    async function record(state) {
        // Persisting the start is required before a mutation can be sent.
        const result = await bounded(() => checkpoint(state));
        if (result.status !== "completed") throw new Error("Recovery checkpoint unavailable; operation not safe to continue.");
    }
    async function run({label, call, mutation = false, expected = null, verify}) {
        if (active) throw new Error("Another guarded operation is still active.");
        if (mutation && unresolvedWrite) throw new Error("Unresolved write; reconcile its remote state before another mutation.");
        if (typeof label !== "string" || !label || typeof call !== "function" || mutation && typeof verify !== "function")
            throw new TypeError("Mutations require a label, call and read-back verifier.");
        active = true;
        const state = {label, mutation, expected};
        try {
            await record({...state, status: "started"});
            if (mutation) unresolvedWrite = state;
            const response = await bounded(call);
            if (response.status !== "completed" || response.value?.isError === true) {
                const status = mutation ? "unknown" : response.status === "completed" ? "failed" : response.status;
                await record({...state, status});
                return {status};
            }
            if (mutation) {
                const confirmation = await bounded(verify);
                if (confirmation.status !== "completed" || confirmation.value !== true) {
                    await record({...state, status: "unknown"});
                    return {status: "unknown"};
                }
                await record({...state, status: "verified"});
                unresolvedWrite = null;
            } else await record({...state, status: "completed"});
            return {status: mutation ? "verified" : "completed", value: response.value};
        } finally {active = false;}
    }
    async function reconcile(verify) {
        if (active) throw new Error("Another guarded operation is still active.");
        if (!unresolvedWrite) return {status: "nothing_pending"};
        active = true;
        try {
            const confirmation = await bounded(verify);
            if (confirmation.status === "completed" && confirmation.value === true) {
                await record({...unresolvedWrite, status: "verified"});
                unresolvedWrite = null;
                return {status: "verified"};
            }
            // A different SHA might mean a newer concurrent update. It is never
            // permission to reset/force a ref or repeat the previous mutation.
            await record({...unresolvedWrite, status: "unknown"});
            return {status: "unknown"};
        } finally {active = false;}
    }
    return {run, reconcile, get unresolvedWrite() {return unresolvedWrite;}};
}
