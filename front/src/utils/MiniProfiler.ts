import { globalConfig } from "../config";

interface MiniProfilerClient {
  // MiniProfiler 4.5.4 exposes this method at runtime, although it is private in its TypeScript source.
  fetchResults(ids: string[]): void;
}

declare global {
  interface Window {
    MiniProfiler?: MiniProfilerClient;
  }
}

let clientPromise: Promise<MiniProfilerClient> | undefined;

function getClient(): Promise<MiniProfilerClient> {
  if (window.MiniProfiler) {
    return Promise.resolve(window.MiniProfiler);
  }
  if (clientPromise) {
    return clientPromise;
  }

  clientPromise = new Promise((resolve, reject) => {
    let script = document.getElementById("mini-profiler") as HTMLScriptElement | null;
    const existingScript = !!script;
    if (!script) {
      const path = `${globalConfig.baseURL.replace(/\/+$/, "")}/profiler/`;
      script = document.createElement("script");
      script.id = "mini-profiler";
      script.src = `${path}includes.min.js`;
      Object.assign(script.dataset, {
        path,
        version: "",
        ids: "",
        position: "BottomLeft",
        scheme: "Light",
        authorized: "true",
        trivial: "true",
        children: "true",
        controls: "true",
        maxTraces: "10",
        trivialMilliseconds: "2",
      });
    }

    const timeout = window.setTimeout(() => reject(new Error("MiniProfiler script load timed out")), 15000);
    script.addEventListener("load", () => {
      window.clearTimeout(timeout);
      if (window.MiniProfiler) {
        resolve(window.MiniProfiler);
      } else {
        reject(new Error("MiniProfiler did not initialize"));
      }
    }, { once: true });
    script.addEventListener("error", () => {
      window.clearTimeout(timeout);
      reject(new Error("Failed to load MiniProfiler script"));
    }, { once: true });
    if (!existingScript) {
      document.body.appendChild(script);
    }
  });
  return clientPromise;
}

export function showMiniProfilerResults(header: unknown): void {
  if (!header) return;

  try {
    const ids: unknown = JSON.parse(String(header));
    if (!Array.isArray(ids) || !ids.every(id => typeof id === "string")) {
      throw new Error("Invalid MiniProfiler IDs");
    }
    if (ids.length) {
      getClient().then(client => client.fetchResults(ids)).catch(error => {
        console.error("MiniProfiler:", error);
      });
    }
  } catch (error) {
    console.error("Invalid MiniProfiler response header:", error);
  }
}
