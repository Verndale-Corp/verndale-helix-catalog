﻿using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using Sitecore;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls;
using Sitecore.Web.UI.XamlSharp.Continuations;
using Verndale.Feature.Redirects.Data;
using Convert = System.Convert;

namespace Verndale.Feature.Redirects.Commands
{
	[Serializable]
	public class Edit : Command, ISupportsContinuation
	{
		private Repository _repository;
		protected Repository Repository
		{
			get
			{
				if (_repository == null)
				{
					_repository = new Repository("sitecore_master_index");
				}

				return _repository;
			}
		}


		public override void Execute(CommandContext context)
		{
			Assert.ArgumentNotNull(context, "context");
			string strID = context.Parameters["ID"];
			if (string.IsNullOrEmpty(strID))
			{
				SheerResponse.Alert("Select a redirect first.", new string[0]);
			}
			else
			{
				NameValueCollection parameters = new NameValueCollection();
				parameters["ID"] = strID;
				ClientPipelineArgs args = new ClientPipelineArgs(parameters);
				ContinuationManager.Current.Start(this, "Run", args);
			}
		}


		public override CommandState QueryState(CommandContext context)
		{
			Assert.ArgumentNotNull(context, "context");
			return base.QueryState(context);
		}

		protected void Run(ClientPipelineArgs args)
		{
			Assert.ArgumentNotNull(args, "args");
			AjaxScriptManager ajaxScriptManager = Client.AjaxScriptManager;

			if (args.IsPostBack)
			{
				ID id = new ID(args.Parameters["ID"]);
				//int id = Convert.ToInt32(strid);
				if (!args.HasResult)
				{
					return;
				}
				string results = args.Result;
				if (string.IsNullOrEmpty(results))
				{
					ajaxScriptManager.Alert("Enter an old URL and a new URL for the new redirect.");
					return;
				}

				//results = HttpUtility.HtmlEncode(results);

				string[] values = results.Split('|');
				var oldValue = values[1];
				var newValue = values[2];
				var siteName = values[3];

				if (Regex.IsMatch(oldValue, Data.Constants.HostNameRegex))
				{
					SheerResponse.Alert(Translate.Text("Only partial URLs are allowed in the Old URL field."));
					return;
				}

				if (!Repository.CheckUrlExists(id, oldValue, siteName)) // check if the old redirect exists here
				{
					try
					{
						//values[1] = values[1].Replace("%20", " ");
						Repository.Update(id, siteName, oldValue, newValue, MainUtil.GetBool(Convert.ToInt32(values[0]), false));

						ajaxScriptManager.Dispatch("redirectmanager:refresh");
						return;
					}
					catch (Exception exception)
					{
						ajaxScriptManager.Alert(
							Translate.Text("An error occured while creating the redirect for\"\":\n\n{1}",
										   new object[] { values[1], exception.Message }));
					}
				}
				SheerResponse.Alert(
					Translate.Text("A redirect with the old URL \"{0}\" already exists.", new object[] { values[1] }),
					new string[0]);
			}
			else
			{
				ajaxScriptManager.Alert("Are you sure you want to Edit this redirect?");
				UrlString editRedirectUrl = new UrlString("/sitecore/shell/~/xaml/Sitecore.SitecoreModule.Shell.Redirect.EditRedirect.aspx");
				editRedirectUrl.Parameters["ID"] = args.Parameters["ID"];
				SheerResponse.ShowModalDialog(editRedirectUrl.ToString(), "780", "350", string.Empty, true);
				args.WaitForPostBack();
			}
		}
	}
}