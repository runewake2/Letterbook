using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Letterbook.DocsSsg.StaticSite;

public interface IStaticPage;
public interface IStaticPage<out T> where T : PageModel
{
	// The fully qualified name of the Razor template
	// ex: Letterbook.Docs.Pages.Blog.Index
	public string TemplatePath { get; }
	public IEnumerable<T> GetPageModels();


}