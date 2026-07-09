#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:AiToolDomainService
// Guid:4c7d9b03-847c-4ef8-9607-f7d10347e5b1
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/08 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using System.Text.Json;
using XiHan.BasicApp.AI.Domain.Enums;
using XiHan.BasicApp.AI.Domain.Repositories;

namespace XiHan.BasicApp.AI.Domain.DomainServices.Implementations;

/// <summary>
/// AI 技能领域服务
/// </summary>
public sealed class AiToolDomainService : IAiToolDomainService
{
    private readonly IAiToolRepository _toolRepository;

    /// <summary>
    /// 构造函数
    /// </summary>
    public AiToolDomainService(IAiToolRepository toolRepository)
    {
        _toolRepository = toolRepository;
    }

    /// <inheritdoc />
    public async Task<AiToolCommandResult> CreateToolAsync(AiToolCreateCommand command, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (command.DefaultMaxCalls < 0)
        {
            throw new InvalidOperationException("默认最大调用次数不能小于 0。");
        }

        var toolCode = Required(command.ToolCode, 100, "AI 技能编码不能为空。", "AI 技能编码不能超过 100 个字符。");
        if (await _toolRepository.ExistsCodeAsync(toolCode, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("AI 技能编码已存在。");
        }

        var tool = new Entities.SysAiTool
        {
            ToolCode = toolCode,
            ToolName = Required(command.ToolName, 200, "AI 技能名称不能为空。", "AI 技能名称不能超过 200 个字符。"),
            ToolType = command.ToolType,
            SourceKey = Required(command.SourceKey, 200, "技能来源键不能为空。", "技能来源键不能超过 200 个字符。"),
            SkillName = command.ToolType == AiToolType.BuiltInSkill ? command.SourceKey.Trim() : null,
            Category = Optional(command.Category, 100, "分类不能超过 100 个字符。"),
            Description = Optional(command.Description, 500, "描述不能超过 500 个字符。"),
            InputSchemaJson = OptionalJson(command.InputSchemaJson, "输入 Schema 必须是有效 JSON。"),
            OutputSchemaJson = OptionalJson(command.OutputSchemaJson, "输出 Schema 必须是有效 JSON。"),
            RiskLevel = command.RiskLevel,
            SafetyLevel = command.SafetyLevel,
            RequiresApproval = command.RequiresApproval,
            DefaultMaxCalls = command.DefaultMaxCalls,
            Status = command.Status,
            Remark = Optional(command.Remark, 500, "备注不能超过 500 个字符。")
        };

        await _toolRepository.AddAsync(tool, cancellationToken);
        return new AiToolCommandResult(tool);
    }

    /// <inheritdoc />
    public async Task<AiToolCommandResult> UpdateToolAsync(AiToolUpdateCommand command, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var tool = await GetToolOrThrowAsync(command.BasicId, cancellationToken);
        if (command.DefaultMaxCalls < 0)
        {
            throw new InvalidOperationException("默认最大调用次数不能小于 0。");
        }

        tool.ToolName = Required(command.ToolName, 200, "AI 技能名称不能为空。", "AI 技能名称不能超过 200 个字符。");
        tool.Category = Optional(command.Category, 100, "分类不能超过 100 个字符。");
        tool.Description = Optional(command.Description, 500, "描述不能超过 500 个字符。");
        tool.InputSchemaJson = OptionalJson(command.InputSchemaJson, "输入 Schema 必须是有效 JSON。");
        tool.OutputSchemaJson = OptionalJson(command.OutputSchemaJson, "输出 Schema 必须是有效 JSON。");
        tool.RiskLevel = command.RiskLevel;
        tool.SafetyLevel = command.SafetyLevel;
        tool.RequiresApproval = command.RequiresApproval;
        tool.DefaultMaxCalls = command.DefaultMaxCalls;
        tool.Remark = Optional(command.Remark, 500, "备注不能超过 500 个字符。");

        await _toolRepository.UpdateAsync(tool, cancellationToken);
        return new AiToolCommandResult(tool);
    }

    /// <inheritdoc />
    public async Task<AiToolCommandResult> UpdateToolStatusAsync(AiToolStatusChangeCommand command, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var tool = await GetToolOrThrowAsync(command.BasicId, cancellationToken);
        tool.Status = command.Status;
        tool.Remark = Optional(command.Remark, 500, "备注不能超过 500 个字符。");
        await _toolRepository.UpdateAsync(tool, cancellationToken);
        return new AiToolCommandResult(tool);
    }

    private async Task<Entities.SysAiTool> GetToolOrThrowAsync(long id, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "AI 技能主键必须大于 0。");
        }

        return await _toolRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("AI 技能不存在。");
    }

    private static string Required(string value, int maxLength, string emptyMessage, string tooLongMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(emptyMessage);
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new InvalidOperationException(tooLongMessage);
        }

        return trimmed;
    }

    private static string? Optional(string? value, int maxLength, string tooLongMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new InvalidOperationException(tooLongMessage);
        }

        return trimmed;
    }

    private static string? OptionalJson(string? value, string invalidMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        try
        {
            using var _ = JsonDocument.Parse(trimmed);
            return trimmed;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(invalidMessage, ex);
        }
    }
}
