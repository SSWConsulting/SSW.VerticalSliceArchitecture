using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;

/// <summary>
/// Groups Aspire-managed containers under one Docker Compose "project" in Docker Desktop / OrbStack.
/// <c>aspire run</c> uses Aspire's own orchestrator rather than Compose, so its containers otherwise
/// appear ungrouped at the top level of the Docker UI. Stamping the conventional Compose labels
/// (<c>com.docker.compose.project</c> / <c>.service</c>) folds them into one named group. Cosmetic
/// only — it does not change how Aspire runs the containers; the dashboard stays the source of truth.
/// </summary>
public static class DockerComposeGroupingExtensions
{
    // No docker-compose.yml in this repo, so this name can't clash with a real Compose project.
    public const string DockerProject = "SSW.VSA";

    public static IResourceBuilder<T> InDockerProject<T>(this IResourceBuilder<T> builder, string projectName = DockerProject)
        where T : ContainerResource
        => builder.WithContainerRuntimeArgs(
            "--label", $"com.docker.compose.project={projectName}",
            "--label", $"com.docker.compose.service={builder.Resource.Name}");
}
