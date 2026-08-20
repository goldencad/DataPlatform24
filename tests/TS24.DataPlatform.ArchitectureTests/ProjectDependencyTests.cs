using System.Xml.Linq;
using Xunit;

namespace TS24.DataPlatform.ArchitectureTests;

public sealed class ProjectDependencyTests
{
    private static readonly IReadOnlyDictionary<string, string[]> AllowedReferences =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["TS24.DataPlatform.Foundation"] = [],
            ["TS24.DataPlatform.MasterData.Contracts"] = ["TS24.DataPlatform.Foundation"],
            ["TS24.DataPlatform.MasterData.Domain"] = ["TS24.DataPlatform.Foundation"],
            ["TS24.DataPlatform.MasterData.Application"] =
                ["TS24.DataPlatform.Foundation", "TS24.DataPlatform.MasterData.Contracts", "TS24.DataPlatform.MasterData.Domain"],
            ["TS24.DataPlatform.MasterData.Persistence"] =
                ["TS24.DataPlatform.Foundation", "TS24.DataPlatform.MasterData.Contracts"],
            ["TS24.DataPlatform.Licensing.Contracts"] = ["TS24.DataPlatform.Foundation"],
            ["TS24.DataPlatform.Licensing.Domain"] = ["TS24.DataPlatform.Foundation"],
            ["TS24.DataPlatform.Licensing.Application"] =
                ["TS24.DataPlatform.Foundation", "TS24.DataPlatform.Licensing.Contracts", "TS24.DataPlatform.Licensing.Domain"],
            ["TS24.DataPlatform.Licensing.Persistence"] =
                ["TS24.DataPlatform.Foundation", "TS24.DataPlatform.Licensing.Contracts"],
            ["TS24.DataPlatform.Provider.MariaDb"] = ["TS24.DataPlatform.Foundation"],
            ["TS24.DataPlatform.Provider.MongoDb"] = ["TS24.DataPlatform.Foundation"],
            ["TS24.DataPlatform.Deployment"] = ["TS24.DataPlatform.Foundation"],
        };

    [Fact]
    public void ProductionProjectsHaveOnlyApprovedDirectReferences()
    {
        var projects = Directory.GetFiles(RepositoryRoot(), "*.csproj", SearchOption.AllDirectories)
            .Where(path => path.Contains($"{Path.DirectorySeparatorChar}src{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .ToDictionary(path => Path.GetFileNameWithoutExtension(path), ReadReferences, StringComparer.Ordinal);

        Assert.Equal(AllowedReferences.Keys.Order(), projects.Keys.Order());
        foreach (var (project, expectedReferences) in AllowedReferences)
        {
            Assert.Equal(expectedReferences.Order(), projects[project].Order());
        }
    }

    [Fact]
    public void ProductionProjectsDoNotReferenceExternalPackages()
    {
        foreach (var project in Directory.GetFiles(Path.Combine(RepositoryRoot(), "src"), "*.csproj", SearchOption.AllDirectories))
        {
            var document = XDocument.Load(project);
            Assert.Empty(document.Descendants("PackageReference"));
        }
    }

    [Fact]
    public void ProjectReferenceGraphIsAcyclic()
    {
        var graph = AllowedReferences.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        foreach (var project in graph.Keys)
        {
            Visit(project, graph, [], []);
        }
    }

    private static void Visit(
        string project,
        IReadOnlyDictionary<string, string[]> graph,
        HashSet<string> visiting,
        HashSet<string> visited)
    {
        if (visited.Contains(project)) return;
        Assert.True(visiting.Add(project), $"Circular project reference detected at {project}.");
        foreach (var dependency in graph[project]) Visit(dependency, graph, visiting, visited);
        visiting.Remove(project);
        visited.Add(project);
    }

    private static string[] ReadReferences(string project)
    {
        var document = XDocument.Load(project);
        return document.Descendants("ProjectReference")
            .Select(element => Path.GetFileNameWithoutExtension((string?)element.Attribute("Include")))
            .Where(name => name is not null)
            .Cast<string>()
            .ToArray();
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "DataPlatform24.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Repository root was not found.");
    }
}
