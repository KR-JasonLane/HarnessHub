using HarnessHub.Models.Harness;

namespace HarnessHub.Abstract.Services;

/// <summary>
/// 하네스 파일 유형별 기본 템플릿 콘텐츠를 제공한다.
/// </summary>
public interface IHarnessTemplateService
{
    /// <summary>
    /// 파일 유형에 맞는 기본 템플릿 콘텐츠를 반환한다.
    /// </summary>
    string GetTemplate(HarnessFileType fileType);
}
