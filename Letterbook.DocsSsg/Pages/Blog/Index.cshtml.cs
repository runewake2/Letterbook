using System.Runtime.CompilerServices;
using Letterbook.DocsSsg.Markdown;
using Letterbook.DocsSsg.StaticSite;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Letterbook.DocsSsg.Pages.Blog;

public class Index([FromKeyedServices("_blog")]MarkdownChrono blog) : PageModel, IStaticPage<Index>
{
	public MarkdownChrono Blog { get; } = blog;
	public List<MarkdownDoc> Posts => Blog.Files;

	public string TemplatePath => ProjectRoot();
	public IEnumerable<Index> GetPageModels()
	{
		return [this];
	}

	private static string ProjectRoot([CallerFilePath] string path = "")
	{
		var abs = $"{Path.GetDirectoryName(path) ?? throw new InvalidOperationException()}/{nameof(Index)}.cshtml";

	return Path.GetRelativePath(Path.GetPathRoot(path), abs);
	}
}