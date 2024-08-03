using System.Reflection;
using Letterbook.DocsSsg.Files;
using Letterbook.DocsSsg.Markdown;
using Markdig;
using RazorLight;

namespace Letterbook.DocsSsg;

public static class DependencyInjection
{
	public static RazorLightEngineBuilder AddStaticSite(this IServiceCollection services, string root)
	{
		var builder = new RazorLightEngineBuilder();
		builder.UseFileSystemProject(root)
			.UseMemoryCachingProvider();
		services.AddSingleton<IRazorLightEngine, RazorLightEngine>(_ => builder.Build());

		return builder;
	}

	public static IServiceCollection AddMarkdown<T>(this IServiceCollection services, string key) where T : class, IMarkdownFiles
	{
		return services
			.AddSingleton<T>()
			.AddKeyedSingleton<T>(key, (provider, _) =>
		{
			provider.GetService<T>();
			var t = provider.GetRequiredService<T>();
			t.LoadFrom(key);
			return t;
		});
	}

	public static IServiceCollection AddProjectFiles(this IServiceCollection services)
	{
		return services.AddSingleton<IProjectFiles, ProjectFiles>();
	}

	public static MarkdownPipelineBuilder AddMarkdig(this IServiceCollection services)
	{
		var builder = new MarkdownPipelineBuilder()
			.UseYamlFrontMatter()
			.UseAdvancedExtensions();
		services.AddSingleton<MarkdownPipeline>(_ => builder.Build());

		return builder;
	}
}