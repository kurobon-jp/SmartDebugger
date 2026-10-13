"""Create a NEW, isolated UPM consumer project for package compatibility checks."""
import argparse
import json
import shutil
from pathlib import Path


def prepare(output, unity_version, variant):
    source = Path(__file__).resolve().parents[1]
    output = Path(output).resolve()
    if output.exists():
        raise SystemExit(f"Refusing to overwrite an existing project: {output}")
    package_name = json.loads((source / "Assets/package.json").read_text())["name"]
    packages = output / "Packages"
    packages.mkdir(parents=True)
    shutil.copytree(source / "Assets", packages / package_name,
                    ignore=lambda path, names: ["Samples", "Samples.meta"]
                    if Path(path) == source / "Assets" else [])
    # Import sample scripts into the consumer so version-specific reporter code
    # is compiled as well as the package runtime.
    sample = output / "Assets/SmartDebuggerSample"
    shutil.copytree(source / "Assets/Samples~/Scripts", sample / "Scripts")
    shutil.copy2(source / "Assets/Samples~/Unity.SmartDebugger.Samples.asmdef", sample)
    dependencies = {"com.unity.test-framework": "1.4.6"}
    if variant in ("input-system", "recording", "disabled"):
        dependencies["com.unity.inputsystem"] = (
            "1.19.0" if unity_version.startswith("6000.") else
            "1.4.4" if unity_version.startswith("2022.1.") else "1.7.0"
        )
    if variant in ("recording", "disabled"):
        for name in ("jp.co.cyberagent.instant-replay", "jp.co.cyberagent.instant-replay.dependencies"):
            dependencies[name] = (
                "https://github.com/CyberAgentGameEntertainment/InstantReplay.git"
                f"?path=/Packages/{name}#v1.6.11"
            )
    # This is the initial manifest of a newly generated fixture, never a
    # modification to the development project's manifest or lock file.
    manifest = {
        "dependencies": dependencies,
        "testables": [package_name],
        "scopedRegistries": [{
            "name": "OpenUPM", "url": "https://package.openupm.com",
            "scopes": ["com.github.kurobon", "org.nuget"],
        }],
    }
    (packages / "manifest.json").write_text(json.dumps(manifest, indent=2) + "\n")
    settings = output / "ProjectSettings"
    settings.mkdir()
    (settings / "ProjectVersion.txt").write_text(f"m_EditorVersion: {unity_version}\n")
    defines = {
        "minimal": "SMART_DEBUG_VERIFY_MINIMAL",
        "input-system": "SMART_DEBUG_VERIFY_INPUT_SYSTEM",
        "recording": "SMART_DEBUG_VERIFY_RECORDING",
        "disabled": "DISABLE_SMART_DEBUG",
    }[variant]
    serialized_version = 29 if unity_version.startswith("6000.") else 26
    # Initial settings for this new fixture. Defining symbols here lets Unity
    # evaluate asmdef constraints before the first compilation.
    (settings / "ProjectSettings.asset").write_text(
        "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n--- !u!129 &1\n"
        "PlayerSettings:\n  m_ObjectHideFlags: 0\n"
        f"  serializedVersion: {serialized_version}\n"
        "  companyName: SmartDebugger\n  productName: PackageVerification\n"
        f"  activeInputHandler: {0 if variant == 'minimal' else 1}\n"
        f"  scriptingDefineSymbols:\n    Standalone: {defines}\n"
    )
    (output / "verification-variant.txt").write_text(variant)
    tests = output / "Assets/Verification/Editor"
    tests.mkdir(parents=True)
    shutil.copy2(source / "Tools/PackageVerificationTests.cs", tests)
    (tests / "PackageVerification.Tests.asmdef").write_text(json.dumps({
        "name": "PackageVerification.Tests", "includePlatforms": ["Editor"],
        "optionalUnityReferences": ["TestAssemblies"],
    }, indent=2) + "\n")
    print(output)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", required=True)
    parser.add_argument("--unity-version", required=True)
    parser.add_argument("--variant", choices=("minimal", "input-system", "recording", "disabled"), default="minimal")
    args = parser.parse_args()
    prepare(args.output, args.unity_version, args.variant)
