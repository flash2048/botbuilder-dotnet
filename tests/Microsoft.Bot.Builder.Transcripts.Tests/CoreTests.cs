﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Tests;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Transcripts.Tests
{
    [TestClass]
    public class CoreTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task BotAdapted_Bracketing()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);

            TestAdapter adapter = new TestAdapter()
                .Use(new BeforeAfterMiddleware())
                .Use(new CatchExceptionMiddleware());

            var flow = new TestFlow(adapter, async (context) => {
                var userMessage = (context.Activity as MessageActivity)?.Text;
                switch (userMessage)
                {
                    case "use middleware":
                        await context.SendActivityAsync("using middleware");
                        break;
                    case "catch exception":
                        await context.SendActivityAsync("generating exception");
                        throw new Exception("exception to catch");
                }
            });

            await flow.Test(activities).StartTestAsync();
        }

        public class BeforeAfterMiddleware : IMiddleware
        {
            public async Task OnTurnAsync(ITurnContext context, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
            {
                await context.SendActivityAsync("before message");
                await next(cancellationToken);
                await context.SendActivityAsync("after message");
            }
        }

        public class CatchExceptionMiddleware : IMiddleware
        {
            public async Task OnTurnAsync(ITurnContext context, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
            {
                try
                {
                    await next(cancellationToken);
                }
                catch (Exception ex)
                {
                    await context.SendActivityAsync($"Caught: {ex.Message}");
                }
            }
        }
    }
}
