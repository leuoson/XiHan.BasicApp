#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskPromptRendererTests
// Guid:0d84d26f-f508-41f6-bb1a-551069011f40
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/09 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Application.Services;
using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Enums;
using Xunit;

namespace XiHan.BasicApp.AI.Tests;

public sealed class AiTaskPromptRendererTests
{
    [Fact]
    public async Task RenderAsync_returns_inline_prompt()
    {
        var renderer = new AiTaskPromptRenderer(new FakeAiPromptStore());
        var task = new SysAiTask(1)
        {
            PromptMode = AiTaskPromptMode.Inline,
            PromptText = "Summarize today's orders."
        };

        var prompt = await renderer.RenderAsync(task);

        Assert.Equal("Summarize today's orders.", prompt);
    }

    [Fact]
    public async Task RenderAsync_resolves_prompt_store_content()
    {
        var store = new FakeAiPromptStore();
        store.Add("daily-summary", "Write the daily summary.", "v1");
        var renderer = new AiTaskPromptRenderer(store);
        var task = new SysAiTask(1)
        {
            PromptMode = AiTaskPromptMode.PromptStore,
            PromptCode = "daily-summary",
            PromptVersion = "v1"
        };

        var prompt = await renderer.RenderAsync(task);

        Assert.Equal("Write the daily summary.", prompt);
    }

    [Fact]
    public async Task RenderAsync_throws_clear_message_when_prompt_store_prompt_is_missing()
    {
        var renderer = new AiTaskPromptRenderer(new FakeAiPromptStore());
        var task = new SysAiTask(1)
        {
            PromptMode = AiTaskPromptMode.PromptStore,
            PromptCode = "missing-prompt"
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => renderer.RenderAsync(task));

        Assert.Equal("AI 任务引用的提示词不存在或已禁用。", ex.Message);
    }
}
