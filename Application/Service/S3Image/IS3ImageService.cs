using Application.Abstraction;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Service.S3Image;

public interface IS3ImageService
{
    Task<Result<List<string>>> UploadRegistrationImagesAsync(
        List<IFormFile> images,
        int requestId);

    Task<Result<List<string>>> MoveImagesToUnitAsync(
        List<string> tempS3Keys,
        int unitId);

    Task<Result> DeleteImagesAsync(List<string> s3Keys);

    Task<Result<string>> GetPresignedUrlAsync(string s3Key, int expirationMinutes = 60);

    string GetCloudFrontUrl(string s3Key);



}
