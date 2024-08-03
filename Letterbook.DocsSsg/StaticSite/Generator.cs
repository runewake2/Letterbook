using System.Reflection;
using Letterbook.DocsSsg.Files;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using RazorLight;

namespace Letterbook.DocsSsg.StaticSite;

public class Generator(IRazorLightEngine razor, IProjectFiles files, IServiceProvider services)
{
	public async Task GenerateStaticSite()
	{
		await RenderStaticPages();

	}

	public async Task RenderStaticPages()
	{
		// var AllTypesOfIRepository = GetAllStaticPages();

		var staticPages = GetAllStaticPages();

		foreach (var staticPage in staticPages)
		{
			var modelProvider = (IStaticPage<PageModel>) services.GetRequiredService(staticPage);
			foreach (var model in modelProvider.GetPageModels())
			{
				var html = await razor.CompileRenderAsync(modelProvider.TemplatePath, model);
			}
		}


		// foreach (var staticPage in staticPages)
		// {
		// 	var actionContext = GetActionContext();
		// 	var view = FindView(actionContext, staticPage.Page.Name);
		//
		// 	await using var writer = staticPage.Model == null
		// 		? await RenderViewAsync(view, actionContext, default(object))
		// 		: await RenderViewAsync(view, actionContext, services.GetRequiredService(staticPage.Model));
		//
		// 	files.WriteToDist(view.Path, writer);
		// }

	}

	private IEnumerable<Type> GetAllStaticPages()
	{
		return Assembly.GetAssembly(GetType())!.GetTypes()
			.Where(type => type is { IsAbstract: false, IsInterface: false } &&
			               type.GetInterfaces()
				               .Any(iType => iType.IsGenericType &&
				                             iType.GetGenericTypeDefinition() == typeof(IStaticPage<>)));
	}

	// public async Task<TextWriter> RenderViewAsync<TModel>(IView view, ActionContext actionContext, TModel model)
	// {
	// 	var writer = new StringWriter();
	// 	var viewContext = new ViewContext(
	// 		actionContext,
	// 		view,
	// 		new ViewDataDictionary<TModel>(
	// 			metadataProvider: new EmptyModelMetadataProvider(),
	// 			modelState: new ModelStateDictionary())
	// 		{
	// 			Model = model
	// 		},
	// 		new TempDataDictionary(
	// 			actionContext.HttpContext,
	// 			temp),
	// 		writer,
	// 		new HtmlHelperOptions());
	//
	// 	await view.RenderAsync(viewContext);
	//
	// 	var html = writer.ToString();
	//
	// 	return writer;
	// }
	//
	// private IView FindView(ActionContext actionContext, string viewName)
	// {
	// 	var names = new[] { viewName, viewName.Replace("_", "/"), viewName + ".cshtml", viewName.Replace("_", "/") + ".cshtml" };
	// 	var searchedLocations = new List<string>();
	//
	// 	foreach (var name in names)
	// 	{
	// 		// var pageResult = razor.FindPage(actionContext, name);
	// 		// if (pageResult.Page is { } page)
	// 			// return page;
	// 		// searchedLocations.AddRange(pageResult.SearchedLocations ?? []);
	//
	// 		var getViewResult = razor.GetView(executingFilePath: null, viewPath: name, isMainPage: true);
	// 		if (getViewResult.Success)
	// 		{
	// 			return getViewResult.View;
	// 		}
	//
	// 		searchedLocations.AddRange(getViewResult.SearchedLocations);
	//
	// 		var findViewResult = razor.FindView(actionContext, name, isMainPage: true);
	// 		if (findViewResult.Success)
	// 		{
	// 			return findViewResult.View;
	// 		}
	//
	// 		searchedLocations.AddRange(findViewResult.SearchedLocations);
	// 	}
	//
	// 	var errorMessage = string.Join(
	// 		Environment.NewLine,
	// 		new[] { $"Unable to find view '{viewName}'. The following locations were searched:" }.Concat(searchedLocations));
	//
	// 	throw new InvalidOperationException(errorMessage);
	// }
	//
	// private ActionContext GetActionContext()
	// {
	// 	var httpContext = new DefaultHttpContext
	// 	{
	// 		RequestServices = services.CreateScope().ServiceProvider,
	// 	};
	// 	// httpContext.RequestServices = services.CreateScope().ServiceProvider;
	// 	return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
	// }

	private static IEnumerable<Type> GetTypesWith<TAttribute>(bool inherit)
		where TAttribute : Attribute =>
		AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(a => a.GetTypes())
			.Where(t => t.IsDefined(typeof(TAttribute), inherit));

	private static IEnumerable<Type> GetTypesImplementing<TType>() =>
		AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(a => a.GetTypes())
			.Where(t => t.IsAssignableFrom(typeof(TType)))
			.Where(t => t.IsClass);

	private static IEnumerable<Type> AllTypesImplementing(Type tType) =>
		AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(a => a.GetTypes())
			.Where(t => !t.IsAbstract && t.IsGenericType && t.GetGenericTypeDefinition() == tType);

	// from x in Assembly.GetAssembly(typeof(AnyTypeInTargetAssembly)).GetTypes()
	// let y = x.BaseType
	// where !x.IsAbstract && !x.IsInterface &&
	// y != null && y.IsGenericType &&
	// y.GetGenericTypeDefinition() == typeof(IRepository<>)
	// select x;
}