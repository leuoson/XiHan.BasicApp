#region <<版权版本注释>>

// ----------------------------------------------------------------
// Copyright ©2021-Present ZhaiFanhua All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// FileName:KnowledgeQueryBoundaryTests
// Guid:6c3c57fe-1914-4c87-8c4c-b29fd075e31a
// Author:liushijie
// Email:leuoson.job@gmail.com
// CreateTime:2026/07/09 00:00:00
// ----------------------------------------------------------------

#endregion <<版权版本注释>>

using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using XiHan.BasicApp.AI.Application.AppServices;
using XiHan.BasicApp.AI.Application.Dtos;
using XiHan.BasicApp.AI.Infrastructure.Configuration;
using XiHan.Framework.AI.Abstractions.Chat;
using XiHan.Framework.AI.Abstractions.Rag;
using XiHan.Framework.AI.Abstractions.Rag.Models;
using XiHan.Framework.MultiTenancy.Abstractions;
using Xunit;

namespace XiHan.BasicApp.AI.Tests;

public sealed class KnowledgeQueryBoundaryTests
{
    [Fact]
    public async Task QueryAsync_returns_existing_answer_and_citation_shape()
    {
        var retriever = new FakeKnowledgeRetriever(
        [
            new RetrievedChunk
            {
                DocumentId = "doc-1",
                Index = 2,
                Text = "Knowledge text",
                Title = "Knowledge title",
                Source = "manual.md",
                Score = 0.92
            }
        ]);
        var augmenter = new FakeRagPromptAugmenter("Augmented prompt");
        var aiService = new FakeXiHanAiService("AI answer");
        var service = new KnowledgeQueryAppService(
            retriever,
            augmenter,
            aiService,
            new FakeCurrentTenant(12),
            Options.Create(new XiHanRagOptions { DefaultTopK = 5 }));

        var result = await service.QueryAsync(new KnowledgeQueryDto
        {
            Query = "What is known?",
            TopK = 3,
            Provider = "provider-a",
            Answer = true
        });

        Assert.Equal("AI answer", result.Answer);
        Assert.Single(result.Citations);
        Assert.Equal("doc-1", result.Citations[0].DocumentId);
        Assert.Equal(2, result.Citations[0].Index);
        Assert.Equal("Knowledge title", result.Citations[0].Title);
        Assert.Equal("manual.md", result.Citations[0].Source);
        Assert.Equal("Knowledge text", result.Citations[0].Text);
        Assert.Equal(0.92, result.Citations[0].Score);
        Assert.Equal(1, augmenter.CallCount);
        Assert.Equal(1, aiService.CallCount);
        Assert.Equal("What is known?", retriever.LastQuery);
        Assert.Equal(3, retriever.LastTopK);
        Assert.Equal(12, retriever.LastFilter?.TenantId);
        Assert.Equal("provider-a", retriever.LastProvider);
        Assert.Equal("provider-a", aiService.LastOptions?.Provider);
        Assert.Equal("Augmented prompt", aiService.LastMessages.Single().Text);
    }

    private sealed class FakeKnowledgeRetriever : IKnowledgeRetriever
    {
        private readonly IReadOnlyList<RetrievedChunk> _chunks;

        public string? LastQuery { get; private set; }

        public int? LastTopK { get; private set; }

        public RetrievalFilter? LastFilter { get; private set; }

        public string? LastProvider { get; private set; }

        public FakeKnowledgeRetriever(IReadOnlyList<RetrievedChunk> chunks)
        {
            _chunks = chunks;
        }

        public Task<IReadOnlyList<RetrievedChunk>> RetrieveAsync(
            string query,
            int topK = 5,
            RetrievalFilter? filter = null,
            string? provider = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LastQuery = query;
            LastTopK = topK;
            LastFilter = filter;
            LastProvider = provider;
            return Task.FromResult(_chunks);
        }
    }

    private sealed class FakeRagPromptAugmenter : IRagPromptAugmenter
    {
        private readonly string _prompt;

        public int CallCount { get; private set; }

        public FakeRagPromptAugmenter(string prompt)
        {
            _prompt = prompt;
        }

        public string Augment(string query, IReadOnlyList<RetrievedChunk> context)
        {
            CallCount++;
            return _prompt;
        }
    }

    private sealed class FakeXiHanAiService : IXiHanAiService
    {
        private readonly string _answer;

        public int CallCount { get; private set; }

        public IReadOnlyList<ChatMessage> LastMessages { get; private set; } = [];

        public XiHanChatOptions? LastOptions { get; private set; }

        public FakeXiHanAiService(string answer)
        {
            _answer = answer;
        }

        public Task<ChatResponse> ChatAsync(
            IEnumerable<ChatMessage> messages,
            XiHanChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            LastMessages = messages.ToList();
            LastOptions = options;
            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, _answer)));
        }

        public async IAsyncEnumerable<ChatResponseUpdate> ChatStreamAsync(
            IEnumerable<ChatMessage> messages,
            XiHanChatOptions? options = null,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.CompletedTask;
            yield return new ChatResponseUpdate(ChatRole.Assistant, _answer);
        }
    }

    private sealed class FakeCurrentTenant : ICurrentTenant
    {
        public FakeCurrentTenant(long? id)
        {
            Id = id;
        }

        public bool IsAvailable => Id.HasValue;

        public long? Id { get; private set; }

        public string? Name { get; private set; }

        public IDisposable Change(long? id, string? name = null)
        {
            var previousId = Id;
            var previousName = Name;
            Id = id;
            Name = name;
            return new DisposeAction(() =>
            {
                Id = previousId;
                Name = previousName;
            });
        }
    }

    private sealed class DisposeAction : IDisposable
    {
        private readonly Action _action;

        public DisposeAction(Action action)
        {
            _action = action;
        }

        public void Dispose()
        {
            _action();
        }
    }
}
