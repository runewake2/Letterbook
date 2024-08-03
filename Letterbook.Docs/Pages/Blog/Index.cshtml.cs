using Letterbook.Docs.Markdown;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;

namespace Letterbook.Docs.Pages.Blog;

public class Index([FromServices] MarkdownFilesFactory<Markdown.MarkdownChrono> blog) : Letterbook.Docs.Page
{
	public Markdown.MarkdownChrono Blog { get; set; } = blog.GetMarkdown("_blog");

	public List<Index> GetStaticProps(RenderContext ctx)
	{
		throw new NotImplementedException();
	}
}