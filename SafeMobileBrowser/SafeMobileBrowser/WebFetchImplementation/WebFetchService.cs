﻿// Copyright 2020 MaidSafe.net limited.
//
// This SAFE Network Software is licensed to you under the MIT license <LICENSE-MIT
// http://opensource.org/licenses/MIT> or the Modified BSD license <LICENSE-BSD
// https://opensource.org/licenses/BSD-3-Clause>, at your option. This file may not be copied,
// modified, or distributed except according to those terms. Please review the Licences for the
// specific language governing permissions and limitations relating to use of the SAFE Network
// Software.

using System;
using System.Text;
using System.Threading.Tasks;
using SafeApp.Core;
using SafeMobileBrowser.Helpers;
using SafeMobileBrowser.Models;
using Xamarin.Essentials;

namespace SafeMobileBrowser.WebFetchImplementation
{
    public static class WebFetchService
    {
        private static readonly WebFetch WebFetch = new WebFetch();

        public static async Task<WebFetchResponse> FetchResourceAsync(string url, WebFetchOptions options = null)
        {
            try
            {
                if (!App.IsConnectedToInternet)
                {
                    throw new WebFetchException(
                        ErrorConstants.NoInternetConnectionError,
                        ErrorConstants.NoInternetConnection);
                }

                if (App.AppSession == null)
                {
                    throw new WebFetchException(
                        ErrorConstants.SessionNotAvailableError,
                        ErrorConstants.SessionNotAvailable);
                }

                if (App.AppSession.IsDisconnected)
                {
                    throw new WebFetchException(
                        ErrorConstants.ConnectionFailedError,
                        ErrorConstants.ConnectionFailed);
                }

                return await WebFetch.FetchAsync(url, options);
            }
            catch (WebFetchException ex)
            {
                Logger.Error(ex);

                string htmlErrorPageString;
                switch (ex.ErrorCode)
                {
                    case WebFetchConstants.NoSuchData:
                    case WebFetchConstants.NoSuchEntry:
                    case WebFetchConstants.NoSuchPublicName:
                    case WebFetchConstants.NoSuchServiceName:
                    case WebFetchConstants.FileNotFound:
                        htmlErrorPageString = await CreateHtmlErrorPage(ErrorConstants.PageNotFound, ex.Message);
                        break;
                    case ErrorConstants.SessionNotAvailableError:
                        htmlErrorPageString = await CreateHtmlErrorPage(
                            ErrorConstants.SessionNotAvailableTitle,
                            ErrorConstants.SessionNotAvailableMsg);
                        break;
                    case ErrorConstants.ConnectionFailedError:
                        htmlErrorPageString = await CreateHtmlErrorPage(
                            ErrorConstants.ConnectionFailedTitle,
                            ErrorConstants.ConnectionFailedMsg);
                        break;
                    case ErrorConstants.NoInternetConnectionError:
                        htmlErrorPageString = await CreateHtmlErrorPage(
                            ErrorConstants.NoInternetConnectionTitle,
                            ErrorConstants.NoInternetConnectionMsg);
                        break;
                    default:
                        htmlErrorPageString = await CreateHtmlErrorPage(
                            ErrorConstants.ErrorOccured,
                            ex.Message);
                        break;
                }

                return new WebFetchResponse
                {
                    Data = Encoding.ASCII.GetBytes(htmlErrorPageString),
                    MimeType = ErrorConstants.ErrorPageMimeType
                };
            }
            catch (FfiException ex)
            {
                Logger.Error(ex);

                string htmlErrorPageString;
                if (ex.ErrorCode == -11 &&
                    Connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    htmlErrorPageString = await CreateHtmlErrorPage(
                        ErrorConstants.NoInternetConnectionTitle,
                        ErrorConstants.NoInternetConnectionMsg);
                }
                else
                {
                    htmlErrorPageString = await CreateHtmlErrorPage(
                        ErrorConstants.ErrorOccured,
                        ErrorConstants.UnableToFetchDataMsg);
                }
                return new WebFetchResponse
                {
                    Data = Encoding.ASCII.GetBytes(htmlErrorPageString),
                    MimeType = ErrorConstants.ErrorPageMimeType
                };
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }
        }

        public static async Task<string> CreateHtmlErrorPage(string errorTitle, string errorMessage)
        {
            var htmlString = await FileHelper.ReadAssetFileContentAsync("index.html");
            htmlString = ReplaceHtmlStringContent(htmlString, ErrorConstants.ErrorHeadingText, errorTitle);
            htmlString = ReplaceHtmlStringContent(htmlString, ErrorConstants.ErrorMessageText, errorMessage);
            return htmlString;
        }

        public static string ReplaceHtmlStringContent(string htmlString, string find, string replaceString)
        {
            int index = htmlString.IndexOf(find, StringComparison.Ordinal);
            return htmlString.Remove(index, find.Length).Insert(index, replaceString);
        }
    }
}
