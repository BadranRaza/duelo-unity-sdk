# Releasing

1. Edit only the canonical source under `Assets/Duelo/`.
2. Update `package.json` and `CHANGELOG.md`.
3. Run `./scripts/sync-sdk.sh`.
4. Run `./scripts/test-package.sh`.
5. Open the repository in the declared Unity version and run all EditMode tests.
6. Validate a clean Git-tag install in a consumer project and build WebGL.
7. Commit, push `main`, and create the matching annotated `vX.Y.Z` tag.
8. Push the tag only after every gate passes.

Never move an existing release tag. Consumers pin immutable tags.
