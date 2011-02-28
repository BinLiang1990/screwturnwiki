﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using ScrewTurn.Wiki.PluginFramework;

namespace ScrewTurn.Wiki.Tests {

	[TestFixture]
	public class FormatterTests {

		private MockRepository mocks;

		[Test]
		public void Format() {
			FormattingContext context = FormattingContext.PageContent;
			PageInfo currentPage = null;
			string[] linkedPages = null;

			string output = Formatter.Format(Input, false, context, currentPage, out linkedPages, false);

			// Ignore \r characters
			// Ignore \n characters

			Assert.AreEqual(ExpectedOutput.Replace("\r\n", ""), output.Replace("\r\n", ""), "Formatter output is different from expected output");
		}

		[SetUp]
		public void SetUp() {
			mocks = new MockRepository();

			ISettingsStorageProviderV30 settingsProvider = mocks.StrictMock<ISettingsStorageProviderV30>();
			Expect.Call(settingsProvider.GetSetting("ProcessSingleLineBreaks")).Return("false").Repeat.Any();

			Collectors.SettingsProvider = settingsProvider;

			IPagesStorageProviderV30 pagesProvider = mocks.StrictMock<IPagesStorageProviderV30>();
			Collectors.PagesProviderCollector = new ProviderCollector<IPagesStorageProviderV30>();
			Collectors.PagesProviderCollector.AddProvider(pagesProvider);

			Expect.Call(settingsProvider.GetSetting("DefaultPagesProvider")).Return(pagesProvider.GetType().FullName).Repeat.Any();

			PageInfo page1 = new PageInfo("page1", pagesProvider, DateTime.Now);
			PageContent page1Content = new PageContent(page1, "Page 1", "User", DateTime.Now, "Comment", "Content", null, null);
			Expect.Call(pagesProvider.GetPage("page1")).Return(page1).Repeat.Any();
			Expect.Call(pagesProvider.GetContent(page1)).Return(page1Content).Repeat.Any();

			Expect.Call(pagesProvider.GetPage("page2")).Return(null).Repeat.Any();

			//Pages.Instance = new Pages();

			Host.Instance = new Host();

			Expect.Call(settingsProvider.GetSetting("CacheSize")).Return("100").Repeat.Any();
			Expect.Call(settingsProvider.GetSetting("CacheCutSize")).Return("20").Repeat.Any();

			Expect.Call(settingsProvider.GetSetting("DefaultCacheProvider")).Return(typeof(CacheProvider).FullName).Repeat.Any();

			// Cache needs setting to init
			mocks.Replay(settingsProvider);

			ICacheProviderV30 cacheProvider = new CacheProvider();
			cacheProvider.Init(Host.Instance, "");
			Collectors.CacheProviderCollector = new ProviderCollector<ICacheProviderV30>();
			Collectors.CacheProviderCollector.AddProvider(cacheProvider);

			mocks.Replay(pagesProvider);

			Collectors.FormatterProviderCollector = new ProviderCollector<IFormatterProviderV30>();

			//System.Web.UI.HtmlTextWriter writer = new System.Web.UI.HtmlTextWriter(new System.IO.StreamWriter(new System.IO.MemoryStream()));
			//System.Web.Hosting.SimpleWorkerRequest request = new System.Web.Hosting.SimpleWorkerRequest("Default.aspx", "?Page=MainPage", writer);
			System.Web.HttpContext.Current = new System.Web.HttpContext(new DummyRequest());
		}

		[TearDown]
		public void TearDown() {
			mocks.VerifyAll();
		}

		private const string Input =
@"'''bold''' ''italic'' __underlined__ --striked--
[page1] [page2|title]

@@* item 1
* item 2
second line@@

{|
| cell || other cell
|}";

		private const string ExpectedOutput =
@"<b>bold</b> <i>italic</i> <u>underlined</u> <strike>striked</strike><br /><a class=""pagelink"" href=""page1.ashx"" title=""Page 1"">page1</a> 
<a class=""unknownlink"" href=""page2.ashx"" title=""page2"">title</a><br /><br /><pre>&#42; item 1
&#42; item 2
second line</pre><br /><table><tr><td>cell</td><td>other cell</td></tr></table>";

	}

	public class DummyRequest : System.Web.HttpWorkerRequest {

		public override void EndOfRequest() {
		}

		public override void FlushResponse(bool finalFlush) {
		}

		public override string GetHttpVerbName() {
			return "GET";
		}

		public override string GetHttpVersion() {
			return "1.1";
		}

		public override string GetLocalAddress() {
			return "http://localhost/";
		}

		public override int GetLocalPort() {
			return 80;
		}

		public override string GetQueryString() {
			return "";
		}

		public override string GetRawUrl() {
			return "http://localhost/Default.aspx";
		}

		public override string GetRemoteAddress() {
			return "127.0.0.1";
		}

		public override int GetRemotePort() {
			return 45695;
		}

		public override string GetUriPath() {
			return "/";
		}

		public override void SendKnownResponseHeader(int index, string value) {
		}

		public override void SendResponseFromFile(IntPtr handle, long offset, long length) {
		}

		public override void SendResponseFromFile(string filename, long offset, long length) {
		}

		public override void SendResponseFromMemory(byte[] data, int length) {
		}

		public override void SendStatus(int statusCode, string statusDescription) {
		}

		public override void SendUnknownResponseHeader(string name, string value) {
		}

	}

}
