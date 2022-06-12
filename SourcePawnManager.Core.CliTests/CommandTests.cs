#region copyright

// Copyright (C) 2022 icebear <icebear@icebear.rocks>
// 
// This file is part of SourcePawnManager (spm).
// 
// SourcePawnManager (spm) is free software: you can redistribute it and/or modify it under the terms of the GNU Affero General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// 
// SourcePawnManager (spm) is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with SourcePawnManager (spm). If not, see <https://www.gnu.org/licenses/>. 

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace SourcePawnManager.Core.CliTests;

public class CommandTests
{
    private static readonly string[] ExpectBothPlugins          = { "tf2attributes", "SM-TFCustAttr" };
    private static readonly string[] ExpectOnlyCustomAttributes = { "-: tf2attributes", "SM-TFCustAttr" };
    private static readonly string[] ExpectOnlyTf2Attributes    = { "tf2attributes", "-: SM-TFCustAttr" };
    private static readonly string[] ExpectNoPlugins            = { "-: tf2attributes", "-: SM-TFCustAttr" };

    private static readonly object[] Cases =
    {
        new object[]
        {
            "empty spm", "show-license",
            new Dictionary<string, string[]?>
            {
                { "spm.json", ExpectNoPlugins },
                { "-: spm.local.json", null },
                { "-: spm.lock.json", null },
                { "-: include/tf2attributes.inc", null },
                { "-: include/tf2attributes.inc.version", null },
                { "-: include/tf_custom_attributes.inc", null },
                { "-: include/tf_custom_attributes.inc.version", null },
            },
            new []
            {
                "SourcePawnManager (spm) is free software",
                "GNU Affero General Public License",
                "https://www.gnu.org/licenses",
                "https://github.com/eisbaer66/spm",
            },
        },
        new object[]
        {
            "empty spm", "list",
            new Dictionary<string, string[]?>
            {
                { "spm.json", ExpectNoPlugins },
                { "-: spm.local.json", null },
                { "-: spm.lock.json", null },
                { "-: include/tf2attributes.inc", null },
                { "-: include/tf2attributes.inc.version", null },
                { "-: include/tf_custom_attributes.inc", null },
                { "-: include/tf_custom_attributes.inc.version", null },
            },
            new []{"0 dependencies are currently installed"},
        },
        new object[]
        {
            "empty spm", "restore",
            new Dictionary<string, string[]?>
            {
                { "spm.json", ExpectNoPlugins },
                { "spm.local.json", null },
                { "spm.lock.json", ExpectNoPlugins },
                { "-: include/tf2attributes.inc", null },
                { "-: include/tf2attributes.inc.version", null },
                { "-: include/tf_custom_attributes.inc", null },
                { "-: include/tf_custom_attributes.inc.version", null },
            },
            new[]{"no dependencies restored."},
        },
        
        new object[]
        {
            "filled spm", "list",
            new Dictionary<string, string[]?>
            {
                { "spm.json", ExpectBothPlugins },
                { "-: spm.local.json", null },
                { "-: spm.lock.json", null },
                { "-: include/tf2attributes.inc", null },
                { "-: include/tf2attributes.inc.version", null },
                { "-: include/tf_custom_attributes.inc", null },
                { "-: include/tf_custom_attributes.inc.version", null },
            },
            new[]
            {
                "2 dependencies are currently installed",
                "GitHubTagFile:nosoop/tf2attributes:tf2attributes.inc",
                "GitHubTagZip:nosoop/SM-TFCustAttr:package.zip:scripting/include/tf_custom_attributes.inc",
            },
        },
        new object[]
        {
            "filled spm", "restore",
            new Dictionary<string, string[]?>
            {
                { "spm.json", ExpectBothPlugins },
                { "spm.local.json", null },
                { "spm.lock.json", ExpectBothPlugins },
                { "include/tf_custom_attributes.inc", null },
                { "include/tf_custom_attributes.inc.version", null },
                { "include/tf2attributes.inc", null },
                { "include/tf2attributes.inc.version", null },
            },
            new[]{"2 dependencies restored:"},
        },
        new object[]
        {
            "filled spm", "restore; remove GitHubTagFile:nosoop/tf2attributes:tf2attributes.inc",
            new Dictionary<string, string[]?>
            {
                { "spm.json", ExpectOnlyCustomAttributes },
                { "spm.local.json", null },
                { "spm.lock.json", ExpectOnlyCustomAttributes },
                { "-: include/tf2attributes.inc", null },
                { "-: include/tf2attributes.inc.version", null },
                { "include/tf_custom_attributes.inc", null },
                { "include/tf_custom_attributes.inc.version", null },
            },
            new[]{"2 dependencies restored:", "dependency GitHubTagFile:nosoop/tf2attributes:tf2attributes.inc removed"},
        },
        new object[]
        {
            "filled spm",
            "restore; remove GitHubTagZip:nosoop/SM-TFCustAttr:package.zip:scripting/include/tf_custom_attributes.inc",
            new Dictionary<string, string[]?>
            {
                { "spm.json", ExpectOnlyTf2Attributes },
                { "spm.local.json", null },
                { "spm.lock.json", ExpectOnlyTf2Attributes },
                { "include/tf2attributes.inc", null },
                { "include/tf2attributes.inc.version", null },
                { "-: include/tf_custom_attributes.inc", null },
                { "-: include/tf_custom_attributes.inc.version", null },
            },
            new[]{"2 dependencies restored:", "dependency GitHubTagZip:nosoop/SM-TFCustAttr:package.zip:scripting/include/tf_custom_attributes.inc removed"},
        },
        
        new object[]
        {
            "locked spm", "list",
            new Dictionary<string, string[]?>
            {
                { "spm.json", null },
                { "-: spm.local.json", null },
                { "spm.lock.json", new[] { "\"1.7.1\"", "\"7.0\"" } },
                { "-: include/tf2attributes.inc", null },
                { "-: include/tf2attributes.inc.version", null },
                { "-: include/tf_custom_attributes.inc", null },
                { "-: include/tf_custom_attributes.inc.version", null },
            },
            new[]
            {
                "2 dependencies are currently installed", 
                "GitHubTagFile:nosoop/tf2attributes:tf2attributes.inc",
                "GitHubTagZip:nosoop/SM-TFCustAttr:package.zip:scripting/include/tf_custom_attributes.inc",
            },
        },
        new object[]
        {
            "locked spm", "restore",
            new Dictionary<string, string[]?>
            {
                { "spm.json", null },
                { "spm.local.json", null },
                { "spm.lock.json", new[] { "\"1.7.1\"", "\"7.0\"" } },
                { "include/tf2attributes.inc", null },
                { "include/tf2attributes.inc.version", null },
                { "include/tf_custom_attributes.inc", null },
                { "include/tf_custom_attributes.inc.version", null },
            },
            new[]{"2 dependencies restored:"},
        },
        new object[]
        {
            "locked spm", "restore; update",
            new Dictionary<string, string[]?>
            {
                { "spm.json", null },
                { "spm.local.json", null },
                { "spm.lock.json", new[] { "-: \"1.7.1\"", "-: \"7.0\"" } },
                { "include/tf2attributes.inc", null },
                { "include/tf2attributes.inc.version", null },
                { "include/tf_custom_attributes.inc", null },
                { "include/tf_custom_attributes.inc.version", null },
            },
            new[]{"2 dependencies restored:", "2 dependencies updated:"},
        },
        
        new object[]
        {
            "no spm", "install github-tag-file nosoop tf2attributes 1.* tf2attributes.inc",
            new Dictionary<string, string[]?>
            {
                { "spm.json", ExpectOnlyTf2Attributes },
                { "spm.local.json", null },
                { "spm.lock.json", ExpectOnlyTf2Attributes },
                { "include/tf2attributes.inc", null },
                { "include/tf2attributes.inc.version", null },
                { "-: include/tf_custom_attributes.inc", null },
                { "-: include/tf_custom_attributes.inc.version", null },
            },
            new[]{"dependency GitHubTagFile:nosoop/tf2attributes:tf2attributes.inc installed"},
        },
        new object[]
        {
            "no spm",
            "install github-tag-zip nosoop SM-TFCustAttr 8.* package.zip scripting/include/tf_custom_attributes.inc",
            new Dictionary<string, string[]?>
            {
                { "spm.json", ExpectOnlyCustomAttributes },
                { "spm.local.json", null },
                { "spm.lock.json", ExpectOnlyCustomAttributes },
                { "-: include/tf2attributes.inc", null },
                { "-: include/tf2attributes.inc.version", null },
                { "include/tf_custom_attributes.inc", null },
                { "include/tf_custom_attributes.inc.version", null },
            },
            new[]{"dependency GitHubTagZip:nosoop/SM-TFCustAttr:package.zip:scripting/include/tf_custom_attributes.inc installed"},
        },
        new object[]
        {
            "no spm", "list",
            new Dictionary<string, string[]?>
            {
                { "-: spm.json", null },
                { "-: spm.local.json", null },
                { "-: spm.lock.json", null },
                { "-: include/tf2attributes.inc", null },
                { "-: include/tf2attributes.inc.version", null },
                { "-: include/tf_custom_attributes.inc", null },
                { "-: include/tf_custom_attributes.inc.version", null },
            },
            new []{"0 dependencies are currently installed"},
        },
        new object[]
        {
            "no spm", "restore",
            new Dictionary<string, string[]?>
            {
                { "spm.local.json", null },
                { "spm.lock.json", ExpectNoPlugins },
                { "-: include/tf2attributes.inc", null },
                { "-: include/tf2attributes.inc.version", null },
                { "-: include/tf_custom_attributes.inc", null },
                { "-: include/tf_custom_attributes.inc.version", null },
            },
            new[]{"no dependencies restored."},
        },
    };

    private string _token = null!;


    [SetUp]
    public void Setup()
    {
        var configurationRoot = new ConfigurationBuilder()
                                .AddUserSecrets<CommandTests>()
                                .AddEnvironmentVariables("SPM_")
                                .Build();
        _token = configurationRoot["GitHub:Token"];
    }

    [Test]
    [TestCaseSource(nameof(Cases))]
    public async Task CommandsWritesFiles(string key, string command, IDictionary<string, string[]?> expectedFiles, IEnumerable<string> expectedOutputs)
    {
        var setup        = Path.Combine("Setups", key);
        var invalidChars = Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()).ToArray();
        var folderName   = new string(command.Where(c => !invalidChars.Contains(c)).ToArray());
        var wd           = Path.Combine("wd", key, folderName);
        if (Directory.Exists(wd))
        {
            Directory.Delete(wd, true);
        }

        Directory.CreateDirectory(wd);
        if (Directory.Exists(setup))
        {
            CopyFilesRecursively(setup, wd);
        }

        var outputStringBuilder = new StringBuilder();
        var commands = command.Split(';', StringSplitOptions.RemoveEmptyEntries)
                              .Select(cmd => cmd.Trim());
        foreach (var cmd in commands)
        {
            Console.WriteLine($"starting command: {cmd}");
            var commandOutput = await StartProcess(cmd, wd);
            outputStringBuilder.AppendLine(commandOutput);
        }

        foreach (var expectedFile in expectedFiles)
        {
            string path;
            if (expectedFile.Key.StartsWith("-: "))
            {
                path = Path.Combine(wd, expectedFile.Key[3..]);
                Assert.That(!File.Exists(path), "not expected output file exists: {0}", path);
            }
            else
            {
                path = Path.Combine(wd, expectedFile.Key);
                Assert.That(File.Exists(path), "output file missing: {0}", path);
            }

            if (expectedFile.Value == null)
            {
                continue;
            }

            var content = await File.ReadAllTextAsync(path);
            foreach (var contentFilter in expectedFile.Value)
            {
                if (contentFilter.StartsWith("-: "))
                {
                    var expectedContent = contentFilter[3..];
                    var contains        = content.Contains(expectedContent);
                    Assert.That(!contains, $"file {expectedFile.Key} does contain {expectedContent}");
                }
                else
                {
                    var contains = content.Contains(contentFilter);
                    Assert.That(contains, $"file {expectedFile.Key} does not contain {contentFilter}");
                }
            }
        }

        var output = outputStringBuilder.ToString();
        foreach (var expectedOutput in expectedOutputs)
        {
            if (expectedOutput.StartsWith("-: "))
            {
                var expectedContent = expectedOutput[3..];
                var contains        = output.Contains(expectedContent);
                Assert.That(!contains, $"output does contain {expectedContent}");
            }
            else
            {
                var contains = output.Contains(expectedOutput);
                Assert.That(contains, $"output does not contain {expectedOutput}");
            }
        }
    }

    private async Task<string> StartProcess(string command, string wd)
    {
        var exePath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "spm.exe" : "spm";

        var defaultArgs = " -vvvv ";
        if (!string.IsNullOrEmpty(_token))
        {
            defaultArgs += "-t " + _token;
        }

        var processStartInfo = new ProcessStartInfo
                               {
                                   FileName               = exePath,
                                   WorkingDirectory       = wd,
                                   UseShellExecute        = false,
                                   RedirectStandardOutput = true,
                                   Arguments              = command + defaultArgs,
                               };

        using var process = Process.Start(processStartInfo);
        if (process == null)
        {
            Assert.Fail("process could not be started");
            return string.Empty;
        }

        using (process.StandardOutput)
        {
            var output = await process.StandardOutput.ReadToEndAsync();
            Console.WriteLine(output);

            await process.WaitForExitAsync();

            Assert.AreEqual(0, process.ExitCode, "spm exited unexpectedly");

            return output;
        }
    }

    private static void CopyFilesRecursively(string sourcePath, string targetPath)
    {
        foreach (var dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
        }

        foreach (var newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
        {
            File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
        }
    }
}