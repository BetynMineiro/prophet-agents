using Microsoft.Extensions.DependencyInjection;
using Prophet.Application.AgentPipeline;
using Prophet.Application.AgentPipeline.Agents;
using Prophet.Application.Interfaces.Pipeline;
using Prophet.Application.Services.EntityId;
using Prophet.Application.UserCases.Pipeline;
using Prophet.Application.UserCases.Pipeline.PipelineExecution;
using Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts;
using Prophet.Application.UserCases.Pipeline.ProjectFinalArtifacts.Validators;
using Prophet.Application.UserCases.Pipeline.ProjectHtmlPocs;
using Prophet.Application.UserCases.Pipeline.ProjectHtmlPocs.Validators;
using Prophet.Application.UserCases.Pipeline.ProjectInputs;
using Prophet.Application.UserCases.Pipeline.ProjectInputs.Validators;
using Prophet.Application.UserCases.Pipeline.Projects;
using Prophet.Application.UserCases.Pipeline.Projects.Validators;
using Prophet.Application.UserCases.Pipeline.Validators;
using Prophet.CrossCutting.Validation;

namespace Prophet.Application;

/// <summary>DI for Prophet API host -- pipeline domain use cases only.</summary>
public static class PipelineApplicationModule
{
    public static void AddPipelineApplication(this IServiceCollection services)
    {
        services.AddSingleton<IEntityIdGenerator, EntityIdGenerator>();
        services.AddScoped<IValidationErrorCollector, ValidationErrorCollector>();
        services.AddScoped<ICreatePipelineProjectUseCase, CreatePipelineProjectUseCase>();
        services.AddScoped<IListPipelineProjectsUseCase, ListPipelineProjectsUseCase>();
        services.AddScoped<IGetPipelineProjectUseCase, GetPipelineProjectUseCase>();
        services.AddScoped<IDeletePipelineProjectUseCase, DeletePipelineProjectUseCase>();
        services.AddScoped<IRestorePipelineProjectUseCase, RestorePipelineProjectUseCase>();
        services.AddScoped<IUpdatePipelineProjectUseCase, UpdatePipelineProjectUseCase>();
        services.AddScoped<IValidator<CreatePipelineProjectRequest>, CreatePipelineProjectRequestValidator>();
        services.AddScoped<IValidator<UpdatePipelineProjectRequest>, UpdatePipelineProjectRequestValidator>();
        services.AddScoped<IValidator<PipelineProjectIdQuery>, PipelineProjectIdQueryValidator>();
        services.AddScoped<IListPipelineInputDocumentsUseCase, ListPipelineInputDocumentsUseCase>();
        services.AddScoped<IUploadPipelineInputDocumentsUseCase, UploadPipelineInputDocumentsUseCase>();
        services.AddScoped<IDeletePipelineInputDocumentUseCase, DeletePipelineInputDocumentUseCase>();
        services.AddScoped<IGetPipelineInputDownloadUrlUseCase, GetPipelineInputDownloadUrlUseCase>();
        services.AddScoped<IValidator<UploadPipelineInputDocumentsRequest>, UploadPipelineInputDocumentsRequestValidator>();
        services.AddScoped<IValidator<PipelineInputDocumentIdQuery>, PipelineInputDocumentIdQueryValidator>();
        services.AddScoped<IListPipelineFinalArtifactsUseCase, ListPipelineFinalArtifactsUseCase>();
        services.AddScoped<IUploadPipelineFinalArtifactsUseCase, UploadPipelineFinalArtifactsUseCase>();
        services.AddScoped<IDeletePipelineFinalArtifactUseCase, DeletePipelineFinalArtifactUseCase>();
        services.AddScoped<IGetPipelineFinalArtifactDownloadUrlUseCase, GetPipelineFinalArtifactDownloadUrlUseCase>();
        services.AddScoped<IGetPipelineFinalArtifactContentUseCase, GetPipelineFinalArtifactContentUseCase>();
        services.AddScoped<ISyncPipelineGeneratedFinalArtifactsUseCase, SyncPipelineGeneratedFinalArtifactsUseCase>();
        services.AddScoped<IValidator<UploadPipelineFinalArtifactsRequest>, UploadPipelineFinalArtifactsRequestValidator>();
        services.AddScoped<IValidator<PipelineFinalArtifactIdQuery>, PipelineFinalArtifactIdQueryValidator>();
        services.AddScoped<IListPipelineHtmlPocsUseCase, ListPipelineHtmlPocsUseCase>();
        services.AddScoped<IUploadPipelineHtmlPocUseCase, UploadPipelineHtmlPocUseCase>();
        services.AddScoped<IDeletePipelineHtmlPocUseCase, DeletePipelineHtmlPocUseCase>();
        services.AddScoped<IGetPipelineHtmlPocSignedUrlUseCase, GetPipelineHtmlPocSignedUrlUseCase>();
        services.AddScoped<IGetPipelineHtmlPocContentUseCase, GetPipelineHtmlPocContentUseCase>();
        services.AddScoped<IValidator<PipelineHtmlPocIdQuery>, PipelineHtmlPocIdQueryValidator>();

        services.AddScoped<ICreateArtifactVersionUseCase, CreateArtifactVersionUseCase>();
        services.AddScoped<IListArtifactVersionsUseCase, ListArtifactVersionsUseCase>();
        services.AddScoped<IGetArtifactVersionUseCase, GetArtifactVersionUseCase>();
        services.AddScoped<FileAgent>();
        services.AddScoped<InsightAgent>();
        services.AddScoped<MarketResearchAgent>();
        services.AddScoped<ModelAgent>();
        services.AddScoped<ArchitectureAgent>();
        services.AddScoped<DiagramAgent>();
        services.AddScoped<PocWebAgent>();
        services.AddScoped<PocMobileAgent>();
        services.AddScoped<DocAgent>();
        services.AddScoped<PackagingAgent>();
        services.AddScoped<IPipelineAgentExecutor, PipelineAgentExecutor>();
        services.AddScoped<IRunPipelineUseCase, RunPipelineUseCase>();
        services.AddScoped<IGetPipelineStatusUseCase, GetPipelineStatusUseCase>();
        services.AddScoped<IListPipelineArtifactsUseCase, ListPipelineArtifactsUseCase>();
        services.AddScoped<IGetPipelineArtifactByTypeUseCase, GetPipelineArtifactByTypeUseCase>();
        services.AddScoped<IListPipelineVersionFilesUseCase, ListPipelineVersionFilesUseCase>();
        services.AddScoped<IGetPipelineVersionFileContentUseCase, GetPipelineVersionFileContentUseCase>();
        services.AddScoped<IUploadPipelineVersionFileUseCase, UploadPipelineVersionFileUseCase>();
        services.AddScoped<IAddPipelineArtifactUseCase, AddPipelineArtifactUseCase>();
        services.AddScoped<ISetPipelineRunStepUseCase, SetPipelineRunStepUseCase>();
        services.AddScoped<ClearPipelineOutputsFromStepUseCase>();
        services.AddScoped<CopyPipelineOutputsFromParentVersionUseCase>();
        services.AddScoped<IRefinementStartStepResolver, RefinementStartStepResolver>();
        services.AddScoped<IContinuePipelineStepUseCase, ContinuePipelineStepUseCase>();
        services.AddScoped<IRetryPipelineStepUseCase, RetryPipelineStepUseCase>();
        services.AddScoped<IRewindPipelineToStepUseCase, RewindPipelineToStepUseCase>();
        services.AddScoped<IUploadPipelineInputUseCase, UploadPipelineInputUseCase>();
        services.AddScoped<IRefinePipelineProjectUseCase, RefinePipelineProjectUseCase>();
    }
}
