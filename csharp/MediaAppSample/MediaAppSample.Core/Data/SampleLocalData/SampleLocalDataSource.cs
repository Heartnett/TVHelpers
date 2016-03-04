﻿//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using MediaAppSample.Core.Models;
using MediaAppSample.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MediaAppSample.Core.Data.SampleLocalData
{
    public sealed class SampleLocalDataSource : ClientApiBase, IDataSource
    {
        #region Variables

        private const string DESCRIPTION = "Lorem Ipsum dolor sit amet, consectetur adipiscing elit. Vestibulum fringilla felis diam. Meecenas mauris lorem, iaculis rhoncus volutpat vel, tritique ut sapien. Pellentesque eros neque, pharetra consectetur nisi nec, iaculis finibus lectus. Cras elit odio, ultricies eu fermentum ac, facilisis sed elit. Suspendisse molestie molestie mi vel lobortis. Praesent id bibendum nulla. Phasellus sed tortor purus. Duis ultrices consectetur diam ut commodo. Fusce et vulputate nulla. Maecenas porttitor quam id dui hendrerit, quis tempus mauris rhoncus. Aenean at nisi fermentum eros laoreet fringilla. Sed eu purus sed arcu pulvinar lobortis. Nulla blandit nec augue sit amet ultricies. Aliquam sit amet eleifend lorem. ";
        private const string PATH_ROOT = "ms-appx:///Data/SampleLocalData/";
        private const string IMAGE_PATH_PREFIX = PATH_ROOT + "Images/";
        private const string CAST_IMAGE_PATH_PREFIX = PATH_ROOT + "Cast/";
        private const string MEDIA_PATH_PREFIX = PATH_ROOT + "Videos/";
        private const string MEDIA_PATH = "wildlife.mp4";
        private readonly Random _random = new Random(1234);

        // short-term hardcoding for US 
        private readonly CultureInfo _usCultureInfo = new CultureInfo("en-US");

        private enum Genres
        {
            Drama,
            SciFi,
            Action,
            Crime,
            Family,
            Horror,
            Comedy,
        }

        #endregion

        #region Methods

        #region Account

        /// <summary>
        /// Performs authentication for the application.
        /// </summary>
        /// <param name="vm">Sign in view model instance that contains the user's login information.</param>
        /// <returns>Login reponse with authorization details.</returns>
        public async Task<UserResponse> AuthenticateAsync(AccountSignInViewModel vm, CancellationToken? ct = null)
        {
            var response = new UserResponse() { AccessToken = "1234567890", RefreshToken = "abcdefghijklmnop", ID = vm.Username, Email = vm.Username, FirstName = "John", LastName = "Doe" };
            await Task.Delay(2000, ct.HasValue ? ct.Value : CancellationToken.None);
            return response;

            //this.Client.DefaultRequestHeaders.Add("Authorization", "Basic YzExNGEzM2U4YjNhNDdmY2E3NzBhYmJiMGNlOWE0YjE6NDFjOTcxYTU3NzlhNGZhMGI4NGZmN2EzNTA4NTQ5M2U=");

            //var dic = new Dictionary<string, string>();
            //dic.Add("grant_type", "password");
            //dic.Add("username", vm.Username);
            //dic.Add("password", vm.Password);
            //dic.Add("scope", "streaming");
            //var contents = new HttpFormUrlEncodedContent(dic);

            //HttpStringContent content = new HttpStringContent(message.Stringify(), UnicodeEncoding.Utf8, "application/json");

            //return await this.PostAsync<UserResponse, HttpFormUrlEncodedContent>(URL_ACCOUNT_SIGNIN, contents, SerializerTypes.Json);
        }

        /// <summary>
        /// Performs account creation for the application.
        /// </summary>
        /// <param name="vm">Sign up view model instance containing all the user's registration information.</param>
        /// <returns>Login response and authorization information if the account creation process was successful.</returns>
        public async Task<UserResponse> RegisterAsync(AccountSignUpViewModel vm, CancellationToken? ct = null)
        {
            var response = new UserResponse() { AccessToken = "0987654321", RefreshToken = "qrstuvwxwyz", ID = vm.Username, Email = vm.Username, FirstName = vm.FirstName, LastName = vm.LastName };
            await Task.Delay(2000, ct.HasValue ? ct.Value : CancellationToken.None);
            return response;

            //var dic = new Dictionary<string, string>();
            //dic.Add("grant_type", "password");
            //dic.Add("username", vm.Username);
            //dic.Add("password", vm.Password1);
            //dic.Add("scope", "streaming");
            //var contents = new HttpFormUrlEncodedContent(dic);

            //HttpStringContent content = new HttpStringContent(message.Stringify(), UnicodeEncoding.Utf8, "application/json");

            //return await this.PostAsync<UserResponse, HttpFormUrlEncodedContent>(URL_ACCOUNT_SIGNUP, contents);
        }

        /// <summary>
        /// Requests forgotten account information when a user cannot rememeber their authentication details.
        /// </summary>
        /// <param name="vm">Account forgot view model instance contain partial account details.</param>
        /// <returns>Response information indicating whether the call was successful or not.</returns>
        public async Task<ForgotPasswordResponse> ForgotPasswordAsync(AccountForgotViewModel vm, CancellationToken? ct = null)
        {
            var response = new ForgotPasswordResponse() { IsValid = true, Message = "Your password has been sent to your e-mail!" };
            await Task.Delay(2000, ct.HasValue ? ct.Value : CancellationToken.None);
            return response;
        }

        /// <summary>
        /// Authenticates a user account returned from the Web Account Manager service.
        /// </summary>
        /// <param name="wi">Web account info object instance representing an authenticated WAM user.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Response object from the server.</returns>
        public async Task<UserResponse> AuthenticateAsync(Services.WebAccountManager.WebAccountInfo wi, CancellationToken? ct = null)
        {
            // This logic below should be server side. Token should be used to retrieve MSA and then check to see if Contoso account exists else register new account.

            switch (wi.Type)
            {
                case Services.WebAccountManager.WebAccountTypes.Microsoft:
                    {
                        // Retrieve MSA profile data
                        MicrosoftAccountDetails msa = null;
                        using (var api = new MicrosoftApi())
                        {
                            msa = await api.GetUserProfile(wi.Token, ct);
                        }

                        if (msa == null)
                            throw new Exception("Could not retrieve Microsoft account profile data!");

                        var response = await this.IsMicrosoftAccountRegistered(msa.id, ct);
                        if (response != null)
                        {
                            // User account exists, return response
                            return response;
                        }
                        else
                        {
                            // No account exists, use MSA profile to register user
                            AccountSignUpViewModel vm = new AccountSignUpViewModel();

                            // Set all the MSA data to the ViewModel
                            vm.Populate(msa);

                            // Call the registration API to create a new account and return
                            return await this.RegisterAsync(vm);
                        }
                    }

                default:
                    throw new NotImplementedException(wi.Type.ToString());
            }
        }

        /// <summary>
        /// Checks to see if a MSA account ID is an existing user of this app service or not.
        /// </summary>
        /// <param name="id">Unique MSA account ID.</param>
        /// <param name="ct">Cancelation token.</param>
        /// <returns>Response object from the server.</returns>
        private Task<UserResponse> IsMicrosoftAccountRegistered(string id, CancellationToken? ct = null)
        {
            // TODO server side logic to check if MSA user is existing or not as a user of this application. Returns "false" in this sample.
            return Task.FromResult<UserResponse>(null);
        }

        #endregion

        #region Search

        public Task<IEnumerable<ContentItemBase>> SearchItems(string searchText, CancellationToken? ct)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Movies

        public Task<IEnumerable<MovieModel>> GetMovies()
        {
            // create movies list
            var list = new ObservableCollection<MovieModel>();
            for (var x = 1; x <= 18; x++)
            {
                CreateAndAddItemToList<MovieModel>(list, x);
            }
            return Task.FromResult<IEnumerable<MovieModel>>(list);
        }

        public async Task<MovieModel> GetMovieHero()
        {
            // curate movie hero
            var results = await this.GetMovies();
            return results.FirstOrDefault();
        }

        public async Task<IEnumerable<MovieModel>> GetMoviesNewReleases()
        {
            // curate new release movies
            return await this.GetMovies();
        }

        public async Task<IEnumerable<MovieModel>> GetMoviesFeatured()
        {
            // curate featured movies
            var list = new List<MovieModel>();
            var results = await this.GetMovies();
            list.Add(results.First(o => o.ContentID == "movie02"));
            list.Add(results.First(o => o.ContentID == "movie18"));
            list.Add(results.First(o => o.ContentID == "movie12"));
            list.Add(results.First(o => o.ContentID == "movie13"));
            return list;
        }

        public async Task<IEnumerable<MovieModel>> GetMoviesTrailers()
        {
            // curate movie trailers
            var results = await this.GetMovies();
            return results.OrderByDescending(o => o.Title);
        }

        public async Task<IEnumerable<ContentItemBase>> GetTrailers(string contentID)
        {
            // curate movie trailers
            var results = await this.GetMovies();
            return results.OrderByDescending(o => o.Title);
        }

        #endregion

        #region TV

        public Task<IEnumerable<TvSeriesModel>> GetTvSeries()
        {
            // create TV series
            var list = new ObservableCollection<TvSeriesModel>();
            for (var x = 19; x <= 36; x++)
            {
                CreateAndAddItemToList<TvSeriesModel>(list, x);
            }
            return Task.FromResult<IEnumerable<TvSeriesModel>>(list);
        }

        public async Task<TvSeriesModel> GetTvHero()
        {
            // curate TV hero
            var results = await this.GetTvNewReleases();
            return results.FirstOrDefault();
        }

        public async Task<IEnumerable<TvSeriesModel>> GetTvNewReleases()
        {
            // curate new TV releases
            return await this.GetTvSeries();
        }

        public async Task<IEnumerable<TvSeriesModel>> GetTvFeatured()
        {
            // curate featured TV
            var results = await this.GetTvSeries();
            var tvSeriesModels = results as IList<TvSeriesModel> ?? results.ToList();

            return new List<TvSeriesModel>
            {
                tvSeriesModels.First(o => o.ContentID == "series19"),
                tvSeriesModels.First(o => o.ContentID == "series20"),
                tvSeriesModels.First(o => o.ContentID == "series21"),
                tvSeriesModels.First(o => o.ContentID == "series22")
            };

            //return new List<TvSeriesModel>
            //{
            //    tvSeriesModels.First(o => o.ContentID == "series03"),
            //    tvSeriesModels.First(o => o.ContentID == "series44"),
            //    tvSeriesModels.First(o => o.ContentID == "series31"),
            //    tvSeriesModels.First(o => o.ContentID == "series32")
            //};
        }

        public IEnumerable<SeasonModel> GetSeasons(TvSeriesModel series)
        {
            // curate seasons
            var list = new ObservableCollection<SeasonModel>();
            CreateAndAddItemToList<SeasonModel>(list, 43);
            CreateAndAddItemToList<SeasonModel>(list, 44);
            CreateAndAddItemToList<SeasonModel>(list, 45);
            return list;
        }

        public Task<IEnumerable<TvEpisodeModel>> GetTvInline()
        {
            // Curate inline content
            var list = new ObservableCollection<TvEpisodeModel>();
            CreateAndAddItemToList<TvEpisodeModel>(list, 37);
            CreateAndAddItemToList<TvEpisodeModel>(list, 38);
            CreateAndAddItemToList<TvEpisodeModel>(list, 39);
            CreateAndAddItemToList<TvEpisodeModel>(list, 40);
            CreateAndAddItemToList<TvEpisodeModel>(list, 41);
            CreateAndAddItemToList<TvEpisodeModel>(list, 42);
            return Task.FromResult<IEnumerable<TvEpisodeModel>>(list);
        }

        public Task<IEnumerable<TvEpisodeModel>> GetTvEpisodes()
        {
            // Curate inline content
            var list = new ObservableCollection<TvEpisodeModel>();
            CreateAndAddItemToList<TvEpisodeModel>(list, 56);
            CreateAndAddItemToList<TvEpisodeModel>(list, 57);
            CreateAndAddItemToList<TvEpisodeModel>(list, 58);
            CreateAndAddItemToList<TvEpisodeModel>(list, 59);
            CreateAndAddItemToList<TvEpisodeModel>(list, 60);
            CreateAndAddItemToList<TvEpisodeModel>(list, 61);
            return Task.FromResult<IEnumerable<TvEpisodeModel>>(list);
        }

        //public Task LoadEpisodes(SeasonModel season)
        //{
        //    return Task.FromResult<object>(null);
        //}

        #endregion

        #region Ratings/Reviews/Casts

        private static IEnumerable<RatingsAndReviewModel> GetRatings(string contentID)
        {
            return new ObservableCollection<RatingsAndReviewModel>
            {
                new RatingsAndReviewModel()
                {
                    ID = "rating1",
                    ReviewSource = "Rating Source 1",
                    RatingDetails = "Rating Source 1 critics",
                    RatingScore = 89,
                    RatingScale = 100
                },
                new RatingsAndReviewModel()
                {
                    ID = "rating2",
                    ReviewSource = "Rating Source 2",
                    RatingDetails = "Based on 35 critic reviews",
                    RatingScore = 74,
                    RatingScale = 100
                },
                new RatingsAndReviewModel()
                {
                    ID = "rating3",
                    ReviewSource = "Rating Source 3",
                    RatingDetails = "From 15,242 user reviews",
                    RatingScore = 8.1,
                    RatingScale = 10
                }
            };
        }

        private static IEnumerable<RatingsAndReviewModel> GetReviews(string contentID)
        {
            return new ObservableCollection<RatingsAndReviewModel>
            {
                new RatingsAndReviewModel()
                {
                    ID = "review1",
                    ReviewSource = "Newspaper 1",
                    RatingDetails = DESCRIPTION,
                    RatingScore = 3.5,
                    RatingScale = 5
                },
                new RatingsAndReviewModel()
                {
                    ID = "review2",
                    ReviewSource = "Online Source 2",
                    RatingDetails = DESCRIPTION,
                    RatingScore = 4.8,
                    RatingScale = 5
                },
                new RatingsAndReviewModel()
                {
                    ID = "review3",
                    ReviewSource = "Newspaper 3",
                    RatingDetails = DESCRIPTION,
                    RatingScore = 3.5,
                    RatingScale = 5
                },
                //new RatingsAndReviewModel()
                //{
                //    ID = "review4",
                //    ReviewSource = "Online Source 4",
                //    RatingDetails = DESCRIPTION,
                //    RatingScore = 3.9,
                //    RatingScale = 5
                //},
                //new RatingsAndReviewModel()
                //{
                //    ID = "review5",
                //    ReviewSource = "Newspaper 5",
                //    RatingDetails = DESCRIPTION,
                //    RatingScore = 4.1,
                //    RatingScale = 5
                //}
            };
        }

        private static IEnumerable<CastAndCrewModel> GetCast(string contentID)
        {
            return new ObservableCollection<CastAndCrewModel>
            {
                new CastAndCrewModel()
                {
                    ID = "cast1",
                    Name = "Ryan Porter",
                    Role = "Hiro",
                    Image = CAST_IMAGE_PATH_PREFIX + "CastImage_06.jpg",
                    OtherWorks = "Movie 5, Movie 8",
                    Biography = DESCRIPTION
                },
                new CastAndCrewModel()
                {
                    ID = "cast2",
                    Name = "Scott Adsit",
                    Role = "Baymax",
                    Image = CAST_IMAGE_PATH_PREFIX + "CastImage_05.jpg",
                    OtherWorks = "TV Series 3, TV Series 4",
                    Biography = DESCRIPTION
                },
                new CastAndCrewModel()
                {
                    ID = "cast3",
                    Name = "Jenny Chung",
                    Role = "Go Go",
                    Image = CAST_IMAGE_PATH_PREFIX + "CastImage_04.jpg",
                    OtherWorks = "Movie 1, Movie 11, TV Series 6",
                    Biography = DESCRIPTION
                },
                new CastAndCrewModel()
                {
                    ID = "cast4",
                    Name = "Daniel Henry",
                    Role = "Tadeshi",
                    Image = CAST_IMAGE_PATH_PREFIX + "CastImage_03.jpg",
                    OtherWorks = "Movie 4, Movie 9, TV Series 9",
                    Biography = DESCRIPTION
                },
                //new CastAndCrewModel()
                //{
                //    ID = "cast5",
                //    Name = "Cast Member 5",
                //    Role = "Role 5",
                //    Image = CAST_IMAGE_PATH_PREFIX + "CastImage_02.jpg",
                //    OtherWorks = "Movie 17, Movie 14, TV Series 12",
                //    Biography = DESCRIPTION
                //}
            };
        }

        private static IEnumerable<CastAndCrewModel> GetCrew(string contentID)
        {
            return new ObservableCollection<CastAndCrewModel>
            {
                new CastAndCrewModel()
                {
                    ID = "crew1",
                    Name = "Don Hall",
                    Role = "Director / Writer",
                    Image = CAST_IMAGE_PATH_PREFIX + "CastImage_01.jpg",
                    OtherWorks = "Movie 2, Movie 3, Movie 4",
                    Biography = DESCRIPTION
                },
                new CastAndCrewModel()
                {
                    ID = "crew2",
                    Name = "Chris Williams",
                    Role = "Director / Writer",
                    Image = CAST_IMAGE_PATH_PREFIX + "CastImage_02.jpg",
                    OtherWorks = "Movie 6, TV Series 3, Movie 18",
                    Biography = DESCRIPTION
                },
                //new CastAndCrewModel()
                //{
                //    ID = "crew3",
                //    Name = "Creator 3",
                //    Role = "Producer",
                //    Image = CAST_IMAGE_PATH_PREFIX + "CastImage_03.jpg",
                //    OtherWorks = "TV Series 2, Movie 3, Movie 10",
                //    Biography = DESCRIPTION
                //},
                //new CastAndCrewModel()
                //{
                //    ID = "crew4",
                //    Name = "Creator 4",
                //    Role = "Screenplay",
                //    Image = CAST_IMAGE_PATH_PREFIX + "CastImage_04.jpg",
                //    OtherWorks = "Movie 16, TV Series 3, Movie 9",
                //    Biography = DESCRIPTION
                //},
                //new CastAndCrewModel()
                //{
                //    ID = "crew5",
                //    Name = "Creator 5",
                //    Role = "Writer",
                //    Image = CAST_IMAGE_PATH_PREFIX + "CastImage_05.jpg",
                //    OtherWorks = "Movie 15, Movie 3, Movie 7",
                //    Biography = DESCRIPTION
                //},
                //new CastAndCrewModel()
                //{
                //    ID = "crew6",
                //    Name = "Creator 6",
                //    Role = "Screenplay",
                //    Image = CAST_IMAGE_PATH_PREFIX + "CastImage_06.jpg",
                //    OtherWorks = "Movie 12, Movie 3, TV Series 4",
                //    Biography = DESCRIPTION
                //}
            };
        }

        #endregion

        #region Queue

        public async Task<IEnumerable<QueueModel>> GetQueue()
        {
            var movies = (await this.GetMovies()).ToArray();
            var tv = (await this.GetTvSeries()).ToArray();
            var episodes = (await GetTvEpisodes()).ToArray();
            return new List<QueueModel>
            {
                new QueueModel() {Item = movies[9]},
                new QueueModel() {Item = movies[8]},
                new QueueModel() {Item = tv[5]},
                new QueueModel() {Item = movies[10]},
                new QueueModel() {Item = tv[9]},
                new QueueModel() {Item = movies[12]},
                new QueueModel() {Item = tv[11]},
                new QueueModel() {Item = movies[14]},
                new QueueModel() {Item = tv[15]}
            };
        }

        public Task AddToQueue(ContentItemBase item)
        {
            return Task.FromResult<object>(null);
        }

        public Task RemoveFromQueue(ContentItemBase item)
        {
            return Task.FromResult<object>(null);
        }

        #endregion

        #region ContentItemBase

        public async Task<IEnumerable<ContentItemBase>> GetRelated(string id)
        {
            // get some Content to populate Related

            if (id.Contains("series"))
            {
                var results = await this.GetTvSeries();
                return results.OrderBy(o => o.Title);
            }
            else
            {
                var results = await this.GetMovies();
                return results.OrderBy(o => o.Title);
            }
        }

        public async Task<ContentItemBase> GetContentItem(string contentId)
        {
            var results = await this.GetMovies();
            return results.FirstOrDefault(s => s.ContentID == contentId);
        }

        private void CreateAndAddItemToList<T>(ObservableCollection<T> list, int number) where T : class
        {
            var numberString = number.ToString();
            var imageNumber = number.ToString("00");

            if (typeof(T) == typeof(MovieModel))
            {
                var item = new MovieModel()
                {
                    ContentID = "movie" + imageNumber,
                    ContentRating = "G",
                    ItemType = ItemTypes.Movie,
                    Title = "Movie " + numberString,
                    UserRating = 0,
                    AverageUserRating = 3.0 + _random.NextDouble() * 2.0,
                    Genre = ((Genres)_random.Next(6)).ToString(),
                    FeaturedImage = IMAGE_PATH_PREFIX + string.Format("FeaturedImage_2x1_Image{0}.jpg", imageNumber),
                    MediaImage = IMAGE_PATH_PREFIX + string.Format("MediaTileTall_16x9_Image{0}.jpg", imageNumber),
                    LandscapeImage = IMAGE_PATH_PREFIX + string.Format("LandScape_2x1_Image{0}.jpg", imageNumber),
                    PosterImage = IMAGE_PATH_PREFIX + string.Format("PosterArt_2x3_Image{0}.jpg", imageNumber),
                    ResumeImage = IMAGE_PATH_PREFIX + string.Format("ResumeTile_16x9_Image{0}.jpg", imageNumber),
                    InlineImage = IMAGE_PATH_PREFIX + string.Format("Inline_16x9_Image{0}.jpg", imageNumber),
                    TvEpisodeImage = IMAGE_PATH_PREFIX + string.Format("PosterArt_2x3_Image{0}.jpg", imageNumber),
                    URL = MEDIA_PATH_PREFIX + MEDIA_PATH,
                    Description = DESCRIPTION,
                    Length = "120",
                    ReleaseDate = DateTime.Parse("1/15/2015", _usCultureInfo),
                    Cast = new ObservableCollection<CastAndCrewModel>(GetCast("movie" + imageNumber)),
                    Creators = new ObservableCollection<CastAndCrewModel>(GetCrew("movie" + imageNumber)),
                    CriticReviews = new ObservableCollection<RatingsAndReviewModel>(GetReviews("movie" + imageNumber)),
                    ContentRatings = new ObservableCollection<RatingsAndReviewModel>(GetRatings("movie" + imageNumber)),
                    Flag = "Just Added",
                };
                if (number == 1) item.Flag = "Featured";
                list.Add(item as T);
            }

            if (typeof(T) == typeof(TvSeriesModel))
            {
                var item = new TvSeriesModel()
                {
                    ContentID = "series" + imageNumber,
                    ItemType = ItemTypes.TvSeries,
                    Title = "TV Series " + (list.Count + 1).ToString(),
                    UserRating = 0,
                    AverageUserRating = 3.0 + _random.NextDouble() * 2.0,
                    Genre = ((Genres)_random.Next(6)).ToString(),
                    FeaturedImage = IMAGE_PATH_PREFIX + string.Format("FeaturedImage_2x1_Image{0}.jpg", imageNumber),
                    MediaImage = IMAGE_PATH_PREFIX + string.Format("MediaTileTall_16x9_Image{0}.jpg", imageNumber),
                    LandscapeImage = IMAGE_PATH_PREFIX + string.Format("LandScape_2x1_Image{0}.jpg", imageNumber),
                    TvEpisodeImage = IMAGE_PATH_PREFIX + string.Format("TVArt_1x1_Image{0}.jpg", imageNumber),
                    ResumeImage = IMAGE_PATH_PREFIX + string.Format("ResumeTile_16x9_Image{0}.jpg", imageNumber),
                    InlineImage = IMAGE_PATH_PREFIX + string.Format("Inline_16x9_Image{0}.jpg", imageNumber),
                    URL = MEDIA_PATH_PREFIX + MEDIA_PATH,
                    Description = DESCRIPTION,
                    ContentRating = "TV-13",
                    ReleaseDate = "2013 - 2014",
                    Length = "44",
                    Cast = new ObservableCollection<CastAndCrewModel>(GetCast("series" + imageNumber)),
                    Creators = new ObservableCollection<CastAndCrewModel>(GetCrew("series" + imageNumber)),
                    CriticReviews = new ObservableCollection<RatingsAndReviewModel>(GetReviews("series" + imageNumber)),
                    ContentRatings = new ObservableCollection<RatingsAndReviewModel>(GetRatings("series" + imageNumber)),
                    Flag = "Most Popular",
                };
                //item.Seasons = new ObservableCollection<SeasonModel>(this.GetSeasons(item));
                list.Add(item as T);
            }

            if (typeof(T) == typeof(TvEpisodeModel))
            {
                var item = new TvEpisodeModel()
                {
                    ContentID = "episode" + imageNumber,
                    ContentRating = "TV-G",
                    ItemType = ItemTypes.TvEpisode,
                    Title = "Episode " + (list.Count + 1).ToString() + " Title",
                    UserRating = 0,
                    AverageUserRating = 3.0 + _random.NextDouble() * 2.0,
                    Genre = "Drama",
                    FeaturedImage = IMAGE_PATH_PREFIX + string.Format("FeaturedImage_2x1_Image{0}.jpg", imageNumber),
                    MediaImage = IMAGE_PATH_PREFIX + string.Format("MediaTileTall_16x9_Image{0}.jpg", imageNumber),
                    LandscapeImage = IMAGE_PATH_PREFIX + string.Format("LandScape_2x1_Image{0}.jpg", imageNumber),
                    TvEpisodeImage = IMAGE_PATH_PREFIX + string.Format("TVArt_1x1_Image{0}.jpg", imageNumber),
                    ResumeImage = IMAGE_PATH_PREFIX + string.Format("ResumeTile_16x9_Image{0}.jpg", imageNumber),
                    InlineImage = IMAGE_PATH_PREFIX + string.Format("Inline_16x9_Image{0}.jpg", imageNumber),
                    URL = MEDIA_PATH_PREFIX + MEDIA_PATH,
                    Description = DESCRIPTION,
                    Length = "60",
                    SeasonNumber = 3,
                    EpisodeNumber = 1,
                    AirDate = DateTime.Parse("1/15/2015", _usCultureInfo),
                    Cast = new ObservableCollection<CastAndCrewModel>(GetCast("episode" + imageNumber)),
                    Creators = new ObservableCollection<CastAndCrewModel>(GetCrew("episode" + imageNumber)),
                    CriticReviews = new ObservableCollection<RatingsAndReviewModel>(GetReviews("episode" + imageNumber)),
                    ContentRatings = new ObservableCollection<RatingsAndReviewModel>(GetRatings("episode" + imageNumber)),
                };


                list.Add(item as T);
            }

            if (typeof(T) == typeof(SeasonModel))
            {
                var item = new SeasonModel()
                {
                    ContentID = "season" + imageNumber,
                    ContentRating = "TV-G",
                    ItemType = ItemTypes.TvSeries,
                    SeasonNumber = (list.Count + 1),
                    UserRating = 3.5,
                    AverageUserRating = 3.0 + _random.NextDouble() * 2.0,
                    Genre = ((Genres)_random.Next(6)).ToString(),
                    //FeaturedImage = IMAGE_PATH_PREFIX + "featured/hero3_nightfal.png",
                    //MediaImage = IMAGE_PATH_PREFIX + "16-9movie/hero2.png",
                    //SquareThumbnailImage = IMAGE_PATH_PREFIX + "1-1tv/odysseyteam.png",
                    //LandscapeThumbnailImage = IMAGE_PATH_PREFIX + "2-1landscape/taxi.png",
                    Description = DESCRIPTION
                };

                switch (item.SeasonNumber)
                {
                    case 1:
                        CreateAndAddItemToList<TvEpisodeModel>(item.Episodes, 41);
                        CreateAndAddItemToList<TvEpisodeModel>(item.Episodes, 42);
                        CreateAndAddItemToList<TvEpisodeModel>(item.Episodes, 43);
                        break;
                    case 2:
                        CreateAndAddItemToList<TvEpisodeModel>(item.Episodes, 44);
                        CreateAndAddItemToList<TvEpisodeModel>(item.Episodes, 45);
                        CreateAndAddItemToList<TvEpisodeModel>(item.Episodes, 46);
                        break;
                    default:
                        CreateAndAddItemToList<TvEpisodeModel>(item.Episodes, 47);
                        CreateAndAddItemToList<TvEpisodeModel>(item.Episodes, 48);
                        CreateAndAddItemToList<TvEpisodeModel>(item.Episodes, 49);
                        break;
                }

                list.Add(item as T);
            }
        }

        #endregion ContentItemBase

        #endregion

        #region Sample Data

        /// <summary>
        /// Gets sample data.
        /// </summary>
        /// <param name="ct">Cancelation token.</param>
        /// <returns>List of items.</returns>
        public Task<IEnumerable<ContentItemBase>> GetItems(CancellationToken? ct)
        {
            var list = new List<ContentItemBase>();
            //list.Add(new ItemModel() { ID = 1, LineOne = "Runtime One", LineTwo = "Maecenas praesent accumsan bibendum", LineThree = "Facilisi faucibus habitant inceptos interdum lobortis nascetur pharetra placerat pulvinar sagittis senectus sociosqu", Latitude = 40, Longitude = -87 });
            //list.Add(new ItemModel() { ID = 2, LineOne = "Runtime Two", LineTwo = "Dictumst eleifend facilisi faucibus", LineThree = "Suscipit torquent ultrices vehicula volutpat maecenas praesent accumsan bibendum dictumst eleifend facilisi faucibus", Latitude = 1, Longitude = 10 });
            //list.Add(new ItemModel() { ID = 3, LineOne = "Runtime Three", LineTwo = "Habitant inceptos interdum lobortis", LineThree = "Habitant inceptos interdum lobortis nascetur pharetra placerat pulvinar sagittis senectus sociosqu suscipit torquent", Latitude = 2, Longitude = 20 });
            //list.Add(new ItemModel() { ID = 4, LineOne = "Runtime Four", LineTwo = "Nascetur pharetra placerat pulvinar", LineThree = "Ultrices vehicula volutpat maecenas praesent accumsan bibendum dictumst eleifend facilisi faucibus habitant inceptos", Latitude = 3, Longitude = 30 });
            //list.Add(new ItemModel() { ID = 5, LineOne = "Runtime Five", LineTwo = "Maecenas praesent accumsan bibendum", LineThree = "Maecenas praesent accumsan bibendum dictumst eleifend facilisi faucibus habitant inceptos interdum lobortis nascetur", Latitude = 4, Longitude = 40 });
            //list.Add(new ItemModel() { ID = 6, LineOne = "Runtime Six", LineTwo = "Dictumst eleifend facilisi faucibus", LineThree = "Pharetra placerat pulvinar sagittis senectus sociosqu suscipit torquent ultrices vehicula volutpat maecenas praesent", Latitude = 5, Longitude = 50 });
            //list.Add(new ItemModel() { ID = 7, LineOne = "Runtime Seven", LineTwo = "Habitant inceptos interdum lobortis", LineThree = "Accumsan bibendum dictumst eleifend facilisi faucibus habitant inceptos interdum lobortis nascetur pharetra placerat", Latitude = 6, Longitude = 60 });
            //list.Add(new ItemModel() { ID = 8, LineOne = "Runtime Eight", LineTwo = "Nascetur pharetra placerat pulvinar", LineThree = "Pulvinar sagittis senectus sociosqu suscipit torquent ultrices vehicula volutpat maecenas praesent accumsan bibendum", Latitude = 7, Longitude = 70 });
            //list.Add(new ItemModel() { ID = 9, LineOne = "Runtime Nine", LineTwo = "Maecenas praesent accumsan bibendum", LineThree = "Facilisi faucibus habitant inceptos interdum lobortis nascetur pharetra placerat pulvinar sagittis senectus sociosqu", Latitude = 8, Longitude = 80 });
            //list.Add(new ItemModel() { ID = 10, LineOne = "Runtime Ten", LineTwo = "Dictumst eleifend facilisi faucibus", LineThree = "Suscipit torquent ultrices vehicula volutpat maecenas praesent accumsan bibendum dictumst eleifend facilisi faucibus", Latitude = 9, Longitude = 90 });
            //list.Add(new ItemModel() { ID = 11, LineOne = "Runtime Eleven", LineTwo = "Habitant inceptos interdum lobortis", LineThree = "Habitant inceptos interdum lobortis nascetur pharetra placerat pulvinar sagittis senectus sociosqu suscipit torquent", Latitude = 10, Longitude = 100 });
            //list.Add(new ItemModel() { ID = 12, LineOne = "Runtime Twelve", LineTwo = "Nascetur pharetra placerat pulvinar", LineThree = "Ultrices vehicula volutpat maecenas praesent accumsan bibendum dictumst eleifend facilisi faucibus habitant inceptos", Latitude = 11, Longitude = 110 });
            //list.Add(new ItemModel() { ID = 13, LineOne = "Runtime Thirteen", LineTwo = "Maecenas praesent accumsan bibendum", LineThree = "Maecenas praesent accumsan bibendum dictumst eleifend facilisi faucibus habitant inceptos interdum lobortis nascetur", Latitude = 12, Longitude = 120 });
            //list.Add(new ItemModel() { ID = 14, LineOne = "Runtime Fourteen", LineTwo = "Dictumst eleifend facilisi faucibus", LineThree = "Pharetra placerat pulvinar sagittis senectus sociosqu suscipit torquent ultrices vehicula volutpat maecenas praesent", Latitude = 13, Longitude = 130 });
            //list.Add(new ItemModel() { ID = 15, LineOne = "Runtime Fifteen", LineTwo = "Habitant inceptos interdum lobortis", LineThree = "Accumsan bibendum dictumst eleifend facilisi faucibus habitant inceptos interdum lobortis nascetur pharetra placerat", Latitude = 14, Longitude = 140 });
            //list.Add(new ItemModel() { ID = 16, LineOne = "Runtime Sixteen", LineTwo = "Nascetur pharetra placerat pulvinar", LineThree = "Pulvinar sagittis senectus sociosqu suscipit torquent ultrices vehicula volutpat maecenas praesent accumsan bibendum", Latitude = 15, Longitude = 150 });
            if (ct.HasValue) ct.Value.ThrowIfCancellationRequested();
            return Task.FromResult<IEnumerable<ContentItemBase>>(list);
        }

        /// <summary>
        /// Gets a sample data item by ID.
        /// </summary>
        /// <param name="id">ID to retrieve.</param>
        /// <param name="ct">Cancelation token.</param>
        /// <returns>ItemModel instance matching the specified ID.</returns>
        public async Task<ContentItemBase> GetItemByID(string id, CancellationToken? ct)
        {
            var items = await this.GetItems(ct);
            if (ct.HasValue) ct.Value.ThrowIfCancellationRequested();
            await Task.Delay(3000, ct.HasValue ? ct.Value : CancellationToken.None);
            if (ct.HasValue) ct.Value.ThrowIfCancellationRequested();
            return items.FirstOrDefault(f => f.ContentID == id);
        }

        #endregion
    }
}
