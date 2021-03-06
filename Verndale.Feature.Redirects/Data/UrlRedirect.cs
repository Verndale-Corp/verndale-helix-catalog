﻿using Sitecore.ContentSearch;
using Sitecore.ContentSearch.SearchTypes;

namespace Verndale.Feature.Redirects.Data
{
	public class UrlRedirect : SearchResultItem
	{
		[IndexField("site_name")]
		public string SiteName { get; set; }

		[IndexField("original_url")]
		public string OldUrl { get; set; }

		[IndexField("new_url")]
		public string NewUrl { get; set; }

		[IndexField("is_301")]
		public bool IsPermanent { get; set; }

		public string RedirectTypeString
		{
			get { return IsPermanent ? "301" : "302"; }
		}
	}
}
