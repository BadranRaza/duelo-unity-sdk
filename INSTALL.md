# Installation

## Git URL

In Unity Package Manager, choose **Add package from git URL** and enter:

```text
https://github.com/BadranRaza/duelo-unity-sdk.git#v1.0.0
```

Or add the pinned dependency to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.duelo.unity-sdk": "https://github.com/BadranRaza/duelo-unity-sdk.git#v1.0.0"
  },
  "testables": [
    "com.duelo.unity-sdk"
  ]
}
```

`testables` is optional for normal use and required only to run the package's
Editor tests from a consuming project.

Run **DUELO → Setup Project** after install. Commit the generated
`Assets/WebGLTemplates/Duelo/` files and Unity's resolved
`Packages/packages-lock.json`.
