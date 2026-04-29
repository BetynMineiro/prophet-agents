using Prophet.Adapters.Llm;
using Prophet.Application.Interfaces.Llm;
using Prophet.Application.UserCases.Pipeline.PipelineExecution;

namespace Prophet.Tests.Application.Pipeline;

public sealed class RefinementStartStepResolverTests
{
    [Theory]
    [InlineData("Atualizar apenas a documentação README", 8)]
    [InlineData("mudar o modelo de domínio para Billing", 3)]
    [InlineData("market research and competitors", 2)]
    [InlineData("chunk the uploaded file differently", 0)]
    public async Task ResolveStartStepIndexAsync_uses_heuristics_when_llm_disabled(string summary, int expected)
    {
        ILlmAdapter llm = new DisabledLlmAdapter();
        var sut = new RefinementStartStepResolver(llm);

        var step = await sut.ResolveStartStepIndexAsync(summary, CancellationToken.None);

        Assert.Equal(expected, step);
    }

    [Fact]
    public async Task ResolveStartStepIndexAsync_empty_summary_returns_zero()
    {
        var sut = new RefinementStartStepResolver(new DisabledLlmAdapter());

        var step = await sut.ResolveStartStepIndexAsync("   ", CancellationToken.None);

        Assert.Equal(0, step);
    }
}
