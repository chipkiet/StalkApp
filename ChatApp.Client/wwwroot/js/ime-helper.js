// ──────────────────────────────────────────────────────────────────────────────
// IME Composition Helper
// Tracks whether the user is in the middle of composing a character via an
// Input Method Editor (IME) – e.g., Vietnamese Telex/VNI, Japanese, Chinese.
//
// Without this guard, pressing Enter to "commit" a composed character also
// fires Blazor's @onkeydown handler, causing the message to be sent with
// a half-composed (or empty) string.
//
// Usage from Blazor (C#):
//   await JS.InvokeVoidAsync("ChatIME.init", "chat-input-id");
//   bool composing = await JS.InvokeAsync<bool>("ChatIME.isComposing");
// ──────────────────────────────────────────────────────────────────────────────
window.ChatIME = {
    _composing: false,

    // Call once after the input element is rendered.
    init: function (elementId) {
        const el = document.getElementById(elementId);
        if (!el) return;

        // Prevent double-binding on hot-reload / re-render
        if (el._imeInitialised) return;
        el._imeInitialised = true;

        el.addEventListener('compositionstart', () => { this._composing = true; });
        el.addEventListener('compositionend',   () => { this._composing = false; });
    },

    // Returns true while the user is composing (do NOT send the message yet).
    isComposing: function () {
        return this._composing;
    }
};
