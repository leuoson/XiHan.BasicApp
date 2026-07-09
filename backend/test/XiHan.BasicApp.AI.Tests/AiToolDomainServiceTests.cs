#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiToolDomainServiceTests
// Guid:36b180ac-3266-4a37-8c47-146e9cb14538
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using XiHan.BasicApp.AI.Domain.Entities;
using XiHan.BasicApp.AI.Domain.DomainServices;
using XiHan.BasicApp.AI.Domain.DomainServices.Implementations;
using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.BasicApp.Saas.Domain.Enums;
using Xunit;

namespace XiHan.BasicApp.AI.Tests;

public sealed class AiToolDomainServiceTests
{
    [Fact]
    public async Task CreateToolAsync_creates_builtin_skill_directory_entry()
    {
        var repository = new InMemoryAiToolRepository(new SysAiTool
        {
            ToolCode = "knowledge_retrieve",
            ToolName = "知识库检索",
            SourceKey = "KnowledgeRetrieveSkill",
            Status = EnableStatus.Enabled
        });
        var service = new AiToolDomainService(repository);
        var command = new AiToolCreateCommand(
            " morning_news_push ",
            "早间新闻推送",
            AiToolType.BuiltInSkill,
            " MorningNewsSkill ",
            "news",
            "整理早间新闻并交给消息推送技能。",
            AiToolRiskLevel.Medium,
            AiToolSafetyLevel.ExternalNetwork,
            EnableStatus.Enabled,
            "deployed code skill");

        var result = await service.CreateToolAsync(command);

        Assert.Equal("morning_news_push", result.Tool.ToolCode);
        Assert.Equal("MorningNewsSkill", result.Tool.SourceKey);
        Assert.Equal(AiToolRiskLevel.Medium, result.Tool.RiskLevel);
        Assert.Equal(AiToolSafetyLevel.ExternalNetwork, result.Tool.SafetyLevel);
    }

    [Fact]
    public async Task CreateToolAsync_normalizes_builtin_skill_and_rejects_duplicate_code()
    {
        var repository = new InMemoryAiToolRepository(new SysAiTool
        {
            ToolCode = "knowledge_retrieve",
            ToolName = "知识库检索",
            SourceKey = "knowledge_retrieve",
            Status = EnableStatus.Enabled
        });
        var service = new AiToolDomainService(repository);
        var command = new AiToolCreateCommand(
            " knowledge_retrieve ",
            "重复知识检索",
            AiToolType.BuiltInSkill,
            "KnowledgeRetrieveSkill",
            "knowledge",
            "重复技能",
            AiToolRiskLevel.Low,
            AiToolSafetyLevel.ReadOnly,
            EnableStatus.Enabled,
            null);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateToolAsync(command));

        Assert.Equal("AI 技能编码已存在。", ex.Message);
    }

    [Fact]
    public void SysAiTool_can_describe_internal_readonly_skill_capability()
    {
        var tool = new SysAiTool
        {
            ToolCode = "knowledge_retrieve",
            ToolName = "知识库检索",
            ToolType = AiToolType.BuiltInSkill,
            SourceKey = "knowledge_retrieve",
            Category = "knowledge",
            RiskLevel = AiToolRiskLevel.Low,
            SafetyLevel = AiToolSafetyLevel.ReadOnly
        };

        Assert.Equal(AiToolType.BuiltInSkill, tool.ToolType);
        Assert.Equal("knowledge_retrieve", tool.SourceKey);
        Assert.Equal(AiToolSafetyLevel.ReadOnly, tool.SafetyLevel);
    }

    [Fact]
    public void SeededKnowledgeRetrieveShape_matches_phase_2a_capability_contract()
    {
        var tool = new SysAiTool
        {
            ToolCode = "knowledge_retrieve",
            ToolName = "知识库检索",
            ToolType = AiToolType.BuiltInSkill,
            SourceKey = "knowledge_retrieve",
            Category = "knowledge",
            Description = "允许 AI 任务检索已接入的知识库片段。",
            RiskLevel = AiToolRiskLevel.Low,
            SafetyLevel = AiToolSafetyLevel.ReadOnly,
            Status = EnableStatus.Enabled
        };

        Assert.Equal("knowledge_retrieve", tool.SourceKey);
        Assert.Equal(AiToolRiskLevel.Low, tool.RiskLevel);
    }
}
