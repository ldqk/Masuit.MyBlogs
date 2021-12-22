namespace Masuit.MyBlogs.Core.Models.ViewModel
{
    public static class SessionKey
    {
        public const string UserInfo = "userinfo";
        public const string HideCategories = nameof(HideCategories);
        public const string TimeZone = "TimeZone";
        public const string RawIP = "rawip";
        public const string ChallengeMode = nameof(ChallengeMode);
        public const string CaptchaChallenge = nameof(CaptchaChallenge);
        public const string JSChallenge = nameof(JSChallenge);
        public const string ChallengeBypass = "challenge-bypass";
        public const string SafeMode = "safemode";
    }
}