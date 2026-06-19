#region License

// Distributed under the MIT License
// ============================================================
// Copyright (c) Upendo Ventures, LLC
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
// and associated documentation files (the "Software"), to deal in the Software without restriction, 
// including without limitation the rights to use, copy, modify, merge, publish, distribute, 
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or 
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
// THE SOFTWARE.

#endregion

namespace Upendo.Modules.UpendoPrompt.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Text;

    using Dnn.PersonaBar.Library.Prompt;
    using Dnn.PersonaBar.Library.Prompt.Attributes;
    using Dnn.PersonaBar.Library.Prompt.Models;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.FileSystem;
    using Upendo.Modules.UpendoPrompt.Components;
    using Upendo.Modules.UpendoPrompt.Custom;
    using Upendo.Modules.UpendoPrompt.Entities;

    using Constants = Upendo.Modules.UpendoPrompt.Components.Constants;

    /// <summary>
    /// Sets missing user profile photos from Gravatar.
    /// </summary>
    [ConsoleCommand("set-profileimagefromgravatar", Constants.PromptCategory, "SetProfileImageFromGravatar")]
    public class SetProfileImageFromGravatar : PromptBase, IConsoleCommand
    {
        private const string AvatarUrlFormat = "https://0.gravatar.com/avatar/{0}?s={1}&d=404&r=g";
        private const string ContentTypeGif = "image/gif";
        private const string ContentTypeJpeg = "image/jpeg";
        private const string ContentTypePng = "image/png";
        private const string FileNameFormat = "gravatar-{0}{1}";
        private const string OverrideEnabledValue = "true";
        private const string PhotoProfilePropertyName = "Photo";
        private const int AvatarSize = 256;
        private const int MaximumAvatarBytes = 1048576;

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SetProfileImageFromGravatar));
        private static readonly HttpClient HttpClient = new HttpClient();

        [FlagParameter("override", Constants.LocalizationKeys.SetProfileImageFromGravatarOverride, "String", "false", false)]
        private const string FlagOverride = "override";

        private bool OverrideExistingProfileImage { get; set; }

        /// <inheritdoc/>
        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            var overrideValue = this.GetFlagValue(FlagOverride, FlagOverride, string.Empty, false);
            this.OverrideExistingProfileImage = string.Equals(overrideValue, OverrideEnabledValue, StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public override ConsoleResultModel Run()
        {
            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

                var messages = new List<PromptMessage>();
                var users = UserController.GetUsers(this.PortalId).Cast<UserInfo>().ToList();
                var updatedCount = 0;
                var skippedCount = 0;
                var failedCount = 0;

                foreach (var user in users)
                {
                    var result = this.TrySetProfileImage(user);
                    messages.Add(new PromptMessage(result.Message));

                    if (result.Updated)
                    {
                        updatedCount++;
                    }
                    else if (result.Failed)
                    {
                        failedCount++;
                    }
                    else
                    {
                        skippedCount++;
                    }
                }

                var output = string.Format(
                    CultureInfo.CurrentCulture,
                    this.LocalizeString(Constants.LocalizationKeys.SetProfileImageFromGravatarResult),
                    updatedCount,
                    skippedCount,
                    failedCount,
                    users.Count);

                return new CustomConsoleResultModel
                {
                    Output = output,
                    Data = messages,
                    IsError = false
                };
            }
            catch (Exception ex)
            {
                this.LogError(ex);
                return new ConsoleErrorResultModel(string.Concat(Constants.OutputPrefix, this.LocalizeString(Constants.LocalizationKeys.ErrorOccurred)));
            }
        }

        /// <inheritdoc/>
        protected override void LogError(Exception ex)
        {
            if (ex != null)
            {
                Logger.Error(ex.Message, ex);

                if (ex.InnerException != null)
                {
                    Logger.Error(ex.InnerException.Message, ex.InnerException);
                }
            }
        }

        private static string CreateGravatarHash(string email)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            var emailBytes = Encoding.UTF8.GetBytes(normalizedEmail);

            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(emailBytes);
                var hashBuilder = new StringBuilder(hashBytes.Length * 2);

                foreach (var hashByte in hashBytes)
                {
                    hashBuilder.Append(hashByte.ToString("x2", CultureInfo.InvariantCulture));
                }

                return hashBuilder.ToString();
            }
        }

        private static string GetAvatarExtension(string contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
            {
                return string.Empty;
            }

            var normalizedContentType = contentType.Split(';')[0].Trim().ToLowerInvariant();

            switch (normalizedContentType)
            {
                case ContentTypeGif:
                    return ".gif";
                case ContentTypeJpeg:
                    return ".jpg";
                case ContentTypePng:
                    return ".png";
                default:
                    return string.Empty;
            }
        }

        private static bool HasExistingProfileImage(UserInfo user)
        {
            var photoProperty = user.Profile?.GetProperty(PhotoProfilePropertyName);

            if (photoProperty == null || string.IsNullOrWhiteSpace(photoProperty.PropertyValue))
            {
                return false;
            }

            int photoFileId;
            if (!int.TryParse(photoProperty.PropertyValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out photoFileId) || photoFileId <= 0)
            {
                return false;
            }

            return FileManager.Instance.GetFile(photoFileId) != null;
        }

        private static byte[] ReadAvatarBytes(HttpResponseMessage response)
        {
            var contentLength = response.Content.Headers.ContentLength;

            if (contentLength.HasValue && contentLength.Value > MaximumAvatarBytes)
            {
                return null;
            }

            var avatarBytes = response.Content.ReadAsByteArrayAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            return avatarBytes.Length <= MaximumAvatarBytes ? avatarBytes : null;
        }

        private ProfileImageFromGravatarResult TrySetProfileImage(UserInfo user)
        {
            try
            {
                if (user == null)
                {
                    return ProfileImageFromGravatarResult.Skipped(this.LocalizeString(Constants.LocalizationKeys.SetProfileImageFromGravatarSkippedUserMissing));
                }

                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    return ProfileImageFromGravatarResult.Skipped(string.Format(
                        CultureInfo.CurrentCulture,
                        this.LocalizeString(Constants.LocalizationKeys.SetProfileImageFromGravatarSkippedEmailMissing),
                        user.Username));
                }

                if (!this.OverrideExistingProfileImage && HasExistingProfileImage(user))
                {
                    return ProfileImageFromGravatarResult.Skipped(string.Format(
                        CultureInfo.CurrentCulture,
                        this.LocalizeString(Constants.LocalizationKeys.SetProfileImageFromGravatarSkippedExisting),
                        user.Username));
                }

                var hash = CreateGravatarHash(user.Email);
                var avatarUrl = string.Format(CultureInfo.InvariantCulture, AvatarUrlFormat, hash, AvatarSize);
                var response = HttpClient.GetAsync(avatarUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false).GetAwaiter().GetResult();

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return ProfileImageFromGravatarResult.Skipped(string.Format(
                        CultureInfo.CurrentCulture,
                        this.LocalizeString(Constants.LocalizationKeys.SetProfileImageFromGravatarSkippedNotFound),
                        user.Username));
                }

                if (!response.IsSuccessStatusCode)
                {
                    return ProfileImageFromGravatarResult.Failure(string.Format(
                        CultureInfo.CurrentCulture,
                        this.LocalizeString(Constants.LocalizationKeys.SetProfileImageFromGravatarFailedDownload),
                        user.Username,
                        (int)response.StatusCode));
                }

                var extension = GetAvatarExtension(response.Content.Headers.ContentType?.MediaType);
                if (string.IsNullOrWhiteSpace(extension))
                {
                    return ProfileImageFromGravatarResult.Failure(string.Format(
                        CultureInfo.CurrentCulture,
                        this.LocalizeString(Constants.LocalizationKeys.SetProfileImageFromGravatarFailedContentType),
                        user.Username,
                        response.Content.Headers.ContentType?.MediaType ?? this.LocalizeString(Constants.LocalizationKeys.SetProfileImageFromGravatarUnknownContentType)));
                }

                var avatarBytes = ReadAvatarBytes(response);
                if (avatarBytes == null || avatarBytes.Length == 0)
                {
                    return ProfileImageFromGravatarResult.Failure(string.Format(
                        CultureInfo.CurrentCulture,
                        this.LocalizeString(Constants.LocalizationKeys.SetProfileImageFromGravatarFailedSize),
                        user.Username));
                }

                var userFolder = FolderManager.Instance.GetUserFolder(user);
                var fileName = string.Format(CultureInfo.InvariantCulture, FileNameFormat, hash.Substring(0, 12), extension);

                using (var avatarStream = new MemoryStream(avatarBytes))
                {
                    var fileInfo = FileManager.Instance.AddFile(userFolder, fileName, avatarStream, true, false, response.Content.Headers.ContentType.MediaType, user.UserID);
                    user.Profile.SetProfileProperty(PhotoProfilePropertyName, fileInfo.FileId.ToString(CultureInfo.InvariantCulture));
                    ProfileController.UpdateUserProfile(user);
                    UserController.UpdateUser(user.PortalID, user);
                    DataCache.ClearUserCache(user.PortalID, user.Username);

                    return ProfileImageFromGravatarResult.Success(string.Format(
                        CultureInfo.CurrentCulture,
                        this.LocalizeString(Constants.LocalizationKeys.SetProfileImageFromGravatarUpdated),
                        user.Username,
                        fileName));
                }
            }
            catch (Exception ex)
            {
                this.LogError(ex);
                return ProfileImageFromGravatarResult.Failure(string.Format(
                    CultureInfo.CurrentCulture,
                    this.LocalizeString(Constants.LocalizationKeys.SetProfileImageFromGravatarFailedException),
                    user?.Username ?? this.LocalizeString(Constants.LocalizationKeys.SetProfileImageFromGravatarUnknownUser),
                    ex.Message));
            }
        }

        private class ProfileImageFromGravatarResult
        {
            private ProfileImageFromGravatarResult(string message, bool updated, bool failed)
            {
                this.Message = message;
                this.Updated = updated;
                this.Failed = failed;
            }

            public bool Failed { get; }

            public string Message { get; }

            public bool Updated { get; }

            public static ProfileImageFromGravatarResult Failure(string message)
            {
                return new ProfileImageFromGravatarResult(message, false, true);
            }

            public static ProfileImageFromGravatarResult Skipped(string message)
            {
                return new ProfileImageFromGravatarResult(message, false, false);
            }

            public static ProfileImageFromGravatarResult Success(string message)
            {
                return new ProfileImageFromGravatarResult(message, true, false);
            }
        }
    }
}
