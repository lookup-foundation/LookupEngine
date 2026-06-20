# Documentation

These rules govern every piece of prose the project ships: XML doc comments, `README.md`, `CHANGELOG.md`, the GitHub wiki, and the guides in this `docs/` folder. Each format adds its own rules on top of the shared set.

A public-surface change updates `README.md`, `CHANGELOG.md`, and the affected XML docs in the same commit. Documentation that lags the code is a defect.

## Shared Prose Rules

* **State what, not how.** Describe observable behavior and contract, never the implementation. Document that a result is computed lazily when a caller must know, not which collection type or algorithm produces it. A summary survives an implementation rewrite unchanged.
* **Plain technical English.** No corporate jargon, no marketing tone.
* **No filler.** Omit obvious statements and empty phrases such as "Not serialized." State only what a reader cannot infer from the signature.
* **Third-person present indicative.** Write "Computes the bounds", not "Computing the bounds" and not "Compute the bounds". No `-ing` verb form for what a member does.
* **One sentence per line.** Break at sentence boundaries, never at a fixed character width.
* **No dashes or semicolons.** Use separate sentences or commas.
* **No open-ended enumerations.** Do not pad a sentence with "including X, Y, Z" lists. Name the one thing that matters, or state the rule generally.

## XML Doc Comments

* Document every public type, method, and property with a `<summary>` that states what it does.
* **`<summary>` describes the member, not its parameters.** Parameters belong in `<param>`, the return value in `<returns>`, and thrown exceptions in `<exception>`. Do not restate the signature in prose.
* Add `<remarks>` only for a non-trivial constraint a caller needs, such as what a null or empty result means. Keep it to one or two sentences.
* Reference another type or member with `<see cref="..."/>` so renames stay tracked.

## README

Example-driven. Keep the usage sections current with the public API, and update the matching example when a signature or behavior changes.

## CHANGELOG

Record every public-surface change in user-facing terms and name the affected API. Each release is a `# <version>` header. Entries group under the subsections that apply: `## Breaking changes`, `## Engine`, `## Fixes`, `## Performance`, `## Testing`. Write each entry as a release note, not a commit message.

## Wiki

Long-form guides and walkthroughs. Follow the shared prose rules and link into the API rather than restate it.
