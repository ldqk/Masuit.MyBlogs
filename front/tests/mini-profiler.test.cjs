const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const test = require("node:test");
const vm = require("node:vm");
const ts = require("typescript");

function createHarness(baseURL = "/api", existingClient) {
  const source = fs.readFileSync(path.join(__dirname, "../src/utils/MiniProfiler.ts"), "utf8");
  const code = ts.transpileModule(source, {
    compilerOptions: { module: ts.ModuleKind.CommonJS },
  }).outputText;
  const scripts = [];
  const errors = [];
  const window = {
    MiniProfiler: existingClient,
    setTimeout,
    clearTimeout,
  };
  const document = {
    getElementById: () => scripts[0] || null,
    createElement: () => ({
      dataset: {},
      events: {},
      addEventListener(name, callback) { this.events[name] = callback; },
    }),
    body: { appendChild(script) { scripts.push(script); } },
  };
  const exports = {};
  vm.runInNewContext(code, {
    exports,
    require: () => ({ globalConfig: { baseURL } }),
    window,
    document,
    console: { error: (...args) => errors.push(args) },
    Promise,
    JSON,
    Error,
  });
  return { show: exports.showMiniProfilerResults, scripts, errors, window };
}

test("queues API results until the proxied profiler script loads", async () => {
  const harness = createHarness();
  const received = [];
  harness.show('["first"]');
  harness.show('["second"]');
  assert.equal(harness.scripts.length, 1);
  const script = harness.scripts[0];
  assert.equal(script.src, "/api/profiler/includes.min.js");
  assert.equal(script.dataset.path, "/api/profiler/");
  harness.window.MiniProfiler = { fetchResults: ids => received.push(...ids) };
  script.events.load();
  await new Promise(resolve => setImmediate(resolve));
  assert.deepEqual(received, ["first", "second"]);
});

test("reuses the existing Razor profiler", async () => {
  const received = [];
  const harness = createHarness("", { fetchResults: ids => received.push(...ids) });
  harness.show('["request"]');
  await new Promise(resolve => setImmediate(resolve));
  assert.deepEqual(received, ["request"]);
  assert.equal(harness.scripts.length, 0);
});

test("waits for an existing Razor script that is still loading", async () => {
  const harness = createHarness("");
  const script = {
    events: {},
    addEventListener(name, callback) { this.events[name] = callback; },
  };
  harness.scripts.push(script);
  const received = [];
  harness.show('["request"]');
  assert.equal(harness.scripts.length, 1);
  harness.window.MiniProfiler = { fetchResults: ids => received.push(...ids) };
  script.events.load();
  await new Promise(resolve => setImmediate(resolve));
  assert.deepEqual(received, ["request"]);
});

test("reports script loading failures without changing API responses", async () => {
  const harness = createHarness();
  harness.show('["request"]');
  harness.scripts[0].events.error();
  await new Promise(resolve => setImmediate(resolve));
  assert.equal(harness.errors.length, 1);
});

test("ignores absent headers and reports malformed headers", () => {
  const harness = createHarness();
  harness.show(undefined);
  harness.show("not json");
  harness.show('{"id":"invalid"}');
  assert.equal(harness.scripts.length, 0);
  assert.equal(harness.errors.length, 2);
});

test("the Axios interceptor handles both successful and failed responses", async () => {
  const source = fs.readFileSync(path.join(__dirname, "../src/axios/AxiosConfig.ts"), "utf8");
  const code = ts.transpileModule(source, {
    compilerOptions: { module: ts.ModuleKind.CommonJS, esModuleInterop: true },
  }).outputText;
  const handlers = {};
  const api = {
    interceptors: {
      request: { use() {} },
      response: { use(success, failure) { handlers.success = success; handlers.failure = failure; } },
    },
  };
  const ids = [];
  const modules = {
    axios: { create: () => api },
    qs: {},
    "../config": { globalConfig: { baseURL: "", timeOut: 1 } },
    "vue3-toastify": { toast: { error() {} } },
    "../utils/MiniProfiler": { showMiniProfilerResults: header => ids.push(header) },
  };
  vm.runInNewContext(code, {
    exports: {},
    require: name => modules[name],
    Promise,
  });
  const data = { Success: true };
  assert.equal(handlers.success({ data, headers: { "x-miniprofiler-ids": '["success"]' } }), data);
  const error = { response: { status: 500, headers: { "x-miniprofiler-ids": '["error"]' } } };
  await assert.rejects(handlers.failure(error), value => value === error);
  assert.deepEqual(ids, ['["success"]', '["error"]']);
});

test("the development proxy identifies local and remote clients", () => {
  const { onProxyReq } = require("../vue.config.js").devServer.proxy["/api"];
  for (const [address, expected] of [
    ["127.0.0.1", "local"],
    ["::1", "local"],
    ["::ffff:127.0.0.1", "local"],
    ["192.0.2.1", "remote"],
  ]) {
    let header;
    onProxyReq({ setHeader: (name, value) => { header = [name, value]; } }, { socket: { remoteAddress: address } });
    assert.deepEqual(header, ["X-Dev-Proxy-Client", expected]);
  }
});
