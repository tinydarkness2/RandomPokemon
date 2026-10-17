// Reads and saves the visitor's unit choice. Storage can be unavailable
// (private browsing, blocked site data), so every access is guarded.

export function load(key) {
    let saved = null;
    try {
        saved = window.localStorage.getItem(key);
    } catch {
        // Fall back to the browser-language default.
    }
    return { saved, language: navigator.language ?? null };
}

export function save(key, value) {
    try {
        window.localStorage.setItem(key, value);
    } catch {
        // Not remembered this time; the choice still applies until the page closes.
    }
}
