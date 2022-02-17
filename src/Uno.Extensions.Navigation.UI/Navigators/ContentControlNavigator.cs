﻿using Uno.Extensions.Logging;
using Uno.Extensions.Navigation.Regions;
using Uno.Extensions.Navigation.UI;

namespace Uno.Extensions.Navigation.Navigators;

public class ContentControlNavigator : ControlNavigator<ContentControl>
{
	protected override FrameworkElement? CurrentView => Control?.Content as FrameworkElement;

	public ContentControlNavigator(
		ILogger<ContentControlNavigator> logger,
		IRegion region,
		IRouteResolver routeResolver,
		RegionControlProvider controlProvider)
		: base(logger, region, routeResolver, controlProvider.RegionControl as ContentControl)
	{
	}

	protected override bool QualifierIsSupported(Route route) =>
		base.QualifierIsSupported(route) ||
		// "../" (change content) Add support for changing current content
		route.IsChangeContent();

	protected override bool CanNavigateToRoute(Route route)
	{
		if (!base.CanNavigateToRoute(route))
		{
			return false;
		}
		var rm = RouteResolver.Find(route);
		if(rm is null )
		{
			return false;
		}
		return rm?.View?.IsSubclassOf(typeof(FrameworkElement)) ?? true; // Inject a FrameView if no View specified
	}
	protected override async Task<string?> Show(string? path, Type? viewType, object? data)
	{
		if (viewType is null ||
			viewType.IsSubclassOf(typeof(Page)))
		{
			if (Logger.IsEnabled(LogLevel.Error)) Logger.LogErrorMessage($"Missing view for navigation path '{path}'");
			path = viewType is null ? path : default;
			viewType = typeof(UI.Controls.FrameView);
		}

		if (Control is null)
		{
			return string.Empty;
		}

		try
		{
			// Clear all child navigation regions
			Region.Children.Clear();

			if (Logger.IsEnabled(LogLevel.Debug)) Logger.LogDebugMessage($"Creating instance of type '{viewType.Name}'");
			var content = Activator.CreateInstance(viewType);
			if (!string.IsNullOrWhiteSpace(path) &&
					content is UI.Controls.FrameView fe)
			{
				fe.SetName(path ?? string.Empty);
			}
			Control.Content = content;


			if (await (Control.Content as FrameworkElement).EnsureLoaded())
			{
				if (Logger.IsEnabled(LogLevel.Debug)) Logger.LogDebugMessage("Instance created");
				return path;
			}
		}
		catch (Exception ex)
		{
			if (Logger.IsEnabled(LogLevel.Error)) Logger.LogErrorMessage($"Unable to create instance - {ex.Message}");
		}

		// Content not loaded, so dispose of content and remove all children
		Control.Content = null;
		Region.Children.Clear();

		return default;
	}
}
