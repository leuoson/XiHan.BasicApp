#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiTaskRunEventTests
// Guid:648e8fe5-e7e8-4e99-a267-bb1972a51adc
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/10 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Application.Mappers;
using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.Enums;
using Xunit;

namespace XiHan.BasicApp.AI.Tests;

public sealed class AiTaskRunEventTests
{
    [Fact]
    public async Task AddAsync_assigns_monotonic_sequence_per_run()
    {
        var repository = new InMemoryAiTaskRunEventRepository();

        var first = await repository.AddAsync(new SysAiTaskRunEvent
        {
            RunId = 10,
            EventType = AiTaskRunEventType.RunQueued,
            Role = AiTaskRunEventRole.System,
            Content = "queued"
        });
        var second = await repository.AddAsync(new SysAiTaskRunEvent
        {
            RunId = 10,
            EventType = AiTaskRunEventType.RunStarted,
            Role = AiTaskRunEventRole.System,
            Content = "started"
        });
        var otherRun = await repository.AddAsync(new SysAiTaskRunEvent
        {
            RunId = 11,
            EventType = AiTaskRunEventType.RunQueued,
            Role = AiTaskRunEventRole.System,
            Content = "queued"
        });

        Assert.Equal(1, first.Sequence);
        Assert.Equal(2, second.Sequence);
        Assert.Equal(1, otherRun.Sequence);
    }

    [Fact]
    public async Task GetByRunIdAsync_returns_events_after_sequence()
    {
        var repository = new InMemoryAiTaskRunEventRepository();
        await repository.AddAsync(new SysAiTaskRunEvent { RunId = 10, EventType = AiTaskRunEventType.RunQueued, Role = AiTaskRunEventRole.System });
        await repository.AddAsync(new SysAiTaskRunEvent { RunId = 10, EventType = AiTaskRunEventType.RunStarted, Role = AiTaskRunEventRole.System });
        await repository.AddAsync(new SysAiTaskRunEvent { RunId = 10, EventType = AiTaskRunEventType.PromptRendered, Role = AiTaskRunEventRole.System });

        var result = await repository.GetByRunIdAsync(10, afterSequence: 1);

        Assert.Equal([2, 3], result.Select(e => e.Sequence).ToArray());
    }

    [Fact]
    public void ToRunEventDto_maps_auditable_event_fields()
    {
        var created = DateTimeOffset.Parse("2026-07-10T08:00:00+08:00");
        var dto = AiTaskApplicationMapper.ToRunEventDto(new SysAiTaskRunEvent(55)
        {
            RunId = 10,
            Sequence = 3,
            EventType = AiTaskRunEventType.PromptRendered,
            Role = AiTaskRunEventRole.System,
            Content = "prompt rendered",
            PayloadJson = "{\"length\":42}",
            CreatedTime = created
        });

        Assert.Equal(55, dto.BasicId);
        Assert.Equal(10, dto.RunId);
        Assert.Equal(3, dto.Sequence);
        Assert.Equal(AiTaskRunEventType.PromptRendered, dto.EventType);
        Assert.Equal(AiTaskRunEventRole.System, dto.Role);
        Assert.Equal("prompt rendered", dto.Content);
        Assert.Equal("{\"length\":42}", dto.PayloadJson);
        Assert.Equal(created, dto.CreatedTime);
    }
}
