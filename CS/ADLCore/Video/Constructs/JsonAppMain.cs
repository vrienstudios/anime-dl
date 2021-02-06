using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ADLCore.Video.Constructs
{
    public class Datum
    {
    }

    public class HentaiTag
    {
        public int? id { get; set; }
        public string text { get; set; }
    }

    public class HentaiVideo
    {
        public int? id { get; set; }
        public bool? is_visible { get; set; }
        public bool? ismp4 { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
        public DateTime created_at { get; set; }
        public DateTime released_at { get; set; }
        public string description { get; set; }
        public int? views { get; set; }
        public int? interests { get; set; }
        public string poster_url { get; set; }
        public string cover_url { get; set; }
        public bool? is_hard_subtitled { get; set; }
        public string brand { get; set; }
        public int? duration_in_ms { get; set; }
        public bool? is_censored { get; set; }
        public int? rating { get; set; }
        public int? likes { get; set; }
        public int? dislikes { get; set; }
        public int? downloads { get; set; }
        public int? monthly_rank { get; set; }
        public string brand_id { get; set; }
        public string is_banned_in { get; set; }
        public object preview_url { get; set; }
        public object primary_color { get; set; }
        public int? created_at_unix { get; set; }
        public int? released_at_unix { get; set; }
        public List<HentaiTag> hentai_tags { get; set; }
        public List<object> titles { get; set; }
    }

    public class HentaiTag2
    {
        public int? id { get; set; }
        public string text { get; set; }
        public int? count { get; set; }
        public string description { get; set; }
        public string wide_image_url { get; set; }
        public string tall_image_url { get; set; }
    }

    public class HentaiFranchise
    {
        public int? id { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
        public string title { get; set; }
    }

    public class HentaiFranchiseHentaiVideo
    {
        public int? id { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
        public DateTime created_at { get; set; }
        public DateTime released_at { get; set; }
        public int? views { get; set; }
        public int? interests { get; set; }
        public string poster_url { get; set; }
        public string cover_url { get; set; }
        public bool? is_hard_subtitled { get; set; }
        public string brand { get; set; }
        public int? duration_in_ms { get; set; }
        public bool? is_censored { get; set; }
        public int? rating { get; set; }
        public int? likes { get; set; }
        public int? dislikes { get; set; }
        public int? downloads { get; set; }
        public int? monthly_rank { get; set; }
        public string brand_id { get; set; }
        public string is_banned_in { get; set; }
        public object preview_url { get; set; }
        public object primary_color { get; set; }
        public int? created_at_unix { get; set; }
        public int? released_at_unix { get; set; }
    }

    public class HentaiVideoStoryboard
    {
        public int? id { get; set; }
        public int? num_total_storyboards { get; set; }
        public int? sequence { get; set; }
        public string url { get; set; }
        public int? frame_width { get; set; }
        public int? frame_height { get; set; }
        public int? num_total_frames { get; set; }
        public int? num_horizontal_frames { get; set; }
        public int? num_vertical_frames { get; set; }
    }

    public class Brand
    {
        public int? id { get; set; }
        public string title { get; set; }
        public string slug { get; set; }
        public object website_url { get; set; }
        public object logo_url { get; set; }
        public object email { get; set; }
        public int? count { get; set; }
    }

    public class NextHentaiVideo
    {
        public int? id { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
        public DateTime created_at { get; set; }
        public DateTime released_at { get; set; }
        public int? views { get; set; }
        public int? interests { get; set; }
        public string poster_url { get; set; }
        public string cover_url { get; set; }
        public bool? is_hard_subtitled { get; set; }
        public string brand { get; set; }
        public int? duration_in_ms { get; set; }
        public bool? is_censored { get; set; }
        public int? rating { get; set; }
        public int? likes { get; set; }
        public int? dislikes { get; set; }
        public int? downloads { get; set; }
        public int? monthly_rank { get; set; }
        public string brand_id { get; set; }
        public string is_banned_in { get; set; }
        public object preview_url { get; set; }
        public object primary_color { get; set; }
        public int? created_at_unix { get; set; }
        public int? released_at_unix { get; set; }
    }

    public class NextRandomHentaiVideo
    {
        public int? id { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
        public DateTime created_at { get; set; }
        public DateTime released_at { get; set; }
        public int? views { get; set; }
        public int? interests { get; set; }
        public string poster_url { get; set; }
        public string cover_url { get; set; }
        public bool? is_hard_subtitled { get; set; }
        public string brand { get; set; }
        public int? duration_in_ms { get; set; }
        public bool is_censored { get; set; }
        public object rating { get; set; }
        public int? likes { get; set; }
        public int? dislikes { get; set; }
        public int? downloads { get; set; }
        public int? monthly_rank { get; set; }
        public string brand_id { get; set; }
        public string is_banned_in { get; set; }
        public object preview_url { get; set; }
        public object primary_color { get; set; }
        public int? created_at_unix { get; set; }
        public int? released_at_unix { get; set; }
    }

    public class Stream
    {
        public int? id { get; set; }
        public int? server_id { get; set; }
        public string slug { get; set; }
        public string kind { get; set; }
        public string extension { get; set; }
        public string mime_type { get; set; }
        public int? width { get; set; }
        public string height { get; set; }
        public int? duration_in_ms { get; set; }
        public int? filesize_mbs { get; set; }
        public string filename { get; set; }
        public string url { get; set; }
        public bool is_guest_allowed { get; set; }
        public bool is_member_allowed { get; set; }
        public bool is_premium_allowed { get; set; }
        public bool is_downloadable { get; set; }
        public string compatibility { get; set; }
        public int? hv_id { get; set; }
        public int? host_id { get; set; }
        public object sub_domain { get; set; }
        public int? server_sequence { get; set; }
        public string video_stream_group_id { get; set; }
    }

    public class Server
    {
        public int? id { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
        public int? na_rating { get; set; }
        public int? eu_rating { get; set; }
        public int? asia_rating { get; set; }
        public int? sequence { get; set; }
        public bool is_permanent { get; set; }
        public List<Stream> streams { get; set; }
    }

    public class VideosManifest
    {
        public List<Server> servers { get; set; }
    }

    public class Desktop
    {
        public int? id { get; set; }
        public string ad_id { get; set; }
        public string ad_type { get; set; }
        public string placement { get; set; }
        public object image_url { get; set; }
        public string iframe_url { get; set; }
        public object click_url { get; set; }
        public int? width { get; set; }
        public int? height { get; set; }
        public string page { get; set; }
        public string form_factor { get; set; }
        public object video_url { get; set; }
        public int? impressions { get; set; }
        public int? clicks { get; set; }
        public int? seconds { get; set; }
        public object placement_x { get; set; }
    }

    public class Ntv1
    {
        public Desktop desktop { get; set; }
    }

    public class Desktop2
    {
        public int? id { get; set; }
        public string ad_id { get; set; }
        public string ad_type { get; set; }
        public string placement { get; set; }
        public object image_url { get; set; }
        public string iframe_url { get; set; }
        public object click_url { get; set; }
        public int? width { get; set; }
        public int? height { get; set; }
        public string page { get; set; }
        public string form_factor { get; set; }
        public object video_url { get; set; }
        public int? impressions { get; set; }
        public int? clicks { get; set; }
        public int? seconds { get; set; }
        public object placement_x { get; set; }
    }

    public class Ntv2
    {
        public Desktop2 desktop { get; set; }
    }

    public class Mobile
    {
        public int? id { get; set; }
        public string ad_id { get; set; }
        public string ad_type { get; set; }
        public string placement { get; set; }
        public object image_url { get; set; }
        public string iframe_url { get; set; }
        public object click_url { get; set; }
        public int? width { get; set; }
        public int? height { get; set; }
        public string page { get; set; }
        public string form_factor { get; set; }
        public object video_url { get; set; }
        public int? impressions { get; set; }
        public int? clicks { get; set; }
        public int? seconds { get; set; }
        public object placement_x { get; set; }
    }

    public class Desktop3
    {
        public int? id { get; set; }
        public string ad_id { get; set; }
        public string ad_type { get; set; }
        public string placement { get; set; }
        public string image_url { get; set; }
        public string iframe_url { get; set; }
        public string click_url { get; set; }
        public int? width { get; set; }
        public int? height { get; set; }
        public string page { get; set; }
        public string form_factor { get; set; }
        public object video_url { get; set; }
        public int? impressions { get; set; }
        public int? clicks { get; set; }
        public int? seconds { get; set; }
        public object placement_x { get; set; }
    }

    public class Footer0
    {
        public Mobile mobile { get; set; }
        public Desktop3 desktop { get; set; }
    }

    public class Mobile2
    {
        public int? id { get; set; }
        public string ad_id { get; set; }
        public string ad_type { get; set; }
        public string placement { get; set; }
        public string image_url { get; set; }
        public object iframe_url { get; set; }
        public string click_url { get; set; }
        public int? width { get; set; }
        public int? height { get; set; }
        public string page { get; set; }
        public string form_factor { get; set; }
        public object video_url { get; set; }
        public int? impressions { get; set; }
        public int? clicks { get; set; }
        public int? seconds { get; set; }
        public string placement_x { get; set; }
    }

    public class Native0
    {
        public Mobile2 mobile { get; set; }
    }

    public class Mobile3
    {
        public int id { get; set; }
        public string ad_id { get; set; }
        public string ad_type { get; set; }
        public string placement { get; set; }
        public string image_url { get; set; }
        public object iframe_url { get; set; }
        public string click_url { get; set; }
        public int? width { get; set; }
        public int? height { get; set; }
        public string page { get; set; }
        public string form_factor { get; set; }
        public object video_url { get; set; }
        public int? impressions { get; set; }
        public int? clicks { get; set; }
        public int? seconds { get; set; }
        public string placement_x { get; set; }
    }

    public class Native1
    {
        public Mobile3 mobile { get; set; }
    }

    public class Desktop4
    {
        public int? id { get; set; }
        public string ad_id { get; set; }
        public string ad_type { get; set; }
        public string placement { get; set; }
        public string image_url { get; set; }
        public string iframe_url { get; set; }
        public string click_url { get; set; }
        public int? width { get; set; }
        public int? height { get; set; }
        public string page { get; set; }
        public string form_factor { get; set; }
        public object video_url { get; set; }
        public int? impressions { get; set; }
        public int? clicks { get; set; }
        public int? seconds { get; set; }
        public object placement_x { get; set; }
    }

    public class Ntv0
    {
        public Desktop4 desktop { get; set; }
    }

    public class Bs
    {
        public Ntv1 ntv_1 { get; set; }
        public Ntv2 ntv_2 { get; set; }
        public Footer0 footer_0 { get; set; }
        public Native0 native_0 { get; set; }
        public Native1 native_1 { get; set; }
        public Ntv0 ntv_0 { get; set; }
    }

    public class Video
    {
        public string player_base_url { get; set; }
        public HentaiVideo hentai_video { get; set; }
        public List<HentaiTag2> hentai_tags { get; set; }
        public HentaiFranchise hentai_franchise { get; set; }
        public List<HentaiFranchiseHentaiVideo> hentai_franchise_hentai_videos { get; set; }
        public List<HentaiVideoStoryboard> hentai_video_storyboards { get; set; }
        public Brand brand { get; set; }
        public object watch_later_playlist_hentai_videos { get; set; }
        public object like_dislike_playlist_hentai_videos { get; set; }
        public object playlist_hentai_videos { get; set; }
        public object similar_playlists_data { get; set; }
        public NextHentaiVideo next_hentai_video { get; set; }
        public NextRandomHentaiVideo next_random_hentai_video { get; set; }
        public VideosManifest videos_manifest { get; set; }
        public object user_license { get; set; }
        public Bs bs { get; set; }
        public object ap { get; set; }
        public string host { get; set; }
    }

    public class Data
    {
        public Video video { get; set; }
    }

    public class Env
    {
        public long? vhtv_version { get; set; }
        public int? premium_coin_cost { get; set; }
    }

    public class Tab
    {
        public string id { get; set; }
        public string icon { get; set; }
        public string title { get; set; }
    }

    public class AccountDialog
    {
        public bool? is_visible { get; set; }
        public string active_tab_id { get; set; }
        public List<Tab> tabs { get; set; }
    }

    public class ContactUsDialog
    {
        public bool? is_visible { get; set; }
        public bool? is_video_report { get; set; }
        public string subject { get; set; }
        public string email { get; set; }
        public string message { get; set; }
        public bool? is_sent { get; set; }
    }

    public class GeneralConfirmationDialog
    {
        public bool? is_visible { get; set; }
        public bool? is_persistent { get; set; }
        public bool? is_mini_close_button_visible { get; set; }
        public bool? is_cancel_button_visible { get; set; }
        public string cancel_button_text { get; set; }
        public string title { get; set; }
        public string body { get; set; }
        public string confirm_button_text { get; set; }
        public object confirmation_callback { get; set; }
    }

    public class Snackbar
    {
        public int? timeout { get; set; }
        public string context { get; set; }
        public string mode { get; set; }
        public string y { get; set; }
        public string x { get; set; }
        public bool? is_visible { get; set; }
        public string text { get; set; }
    }

    public class Search
    {
        public List<object> cache_sorting_config { get; set; }
        public object cache_tags_filter { get; set; }
        public object cache_active_brands { get; set; }
        public object cache_blacklisted_tags_filter { get; set; }
        public string search_text { get; set; }
        public object search_results { get; set; }
        public int? total_search_results_count { get; set; }
        public string order_by { get; set; }
        public string ordering { get; set; }
        public string tags_match { get; set; }
        public int? page_size { get; set; }
        public int? offset { get; set; }
        public int? page { get; set; }
        public int? number_of_pages { get; set; }
        public List<object> tags { get; set; }
        public int? active_tags_count { get; set; }
        public List<object> brands { get; set; }
        public int? active_brands_count { get; set; }
        public List<object> blacklisted_tags { get; set; }
        public int? active_blacklisted_tags_count { get; set; }
        public bool? is_using_preferences { get; set; }
    }

    public class State
    {
        public int? scrollY { get; set; }
        public long? version { get; set; }
        public bool? is_new_version { get; set; }
        public object r { get; set; }
        public object country_code { get; set; }
        public string page_name { get; set; }
        public string user_agent { get; set; }
        public object ip { get; set; }
        public object referrer { get; set; }
        public object geo { get; set; }
        public bool? is_dev { get; set; }
        public bool? is_wasm_supported { get; set; }
        public bool? is_mounted { get; set; }
        public bool? is_loading { get; set; }
        public bool? is_searching { get; set; }
        public int? browser_width { get; set; }
        public int? browser_height { get; set; }
        public string system_msg { get; set; }
        public Data data { get; set; }
        public object auth_claim { get; set; }
        public string session_token { get; set; }
        public int? session_token_expire_time_unix { get; set; }
        public Env env { get; set; }
        public object user { get; set; }
        public object user_setting { get; set; }
        public object user_search_option { get; set; }
        public object playlists { get; set; }
        public bool? shuffle { get; set; }
        public AccountDialog account_dialog { get; set; }
        public ContactUsDialog contact_us_dialog { get; set; }
        public GeneralConfirmationDialog general_confirmation_dialog { get; set; }
        public Snackbar snackbar { get; set; }
        public Search search { get; set; }
    }

    public class Root
    {
        public string layout { get; set; }
        public string? linkToManifest { get; set; }
        public List<Datum> data { get; set; }
        public object error { get; set; }
        public bool serverRendered { get; set; }
        public State state { get; set; }
    }

    public class SA
    {
        public int id { get; set; }
        public string name { get; set; }
        public List<string> titles { get; set; }
        public string slug { get; set; }
        public string description { get; set; }
        public int views { get; set; }
        public int interests { get; set; }
        public string poster_url { get; set; }
        public string cover_url { get; set; }
        public string brand { get; set; }
        public int brand_id { get; set; }
        public int duration_in_ms { get; set; }
        public bool is_censored { get; set; }
        public object rating { get; set; }
        public int likes { get; set; }
        public int dislikes { get; set; }
        public int downloads { get; set; }
        public int monthly_rank { get; set; }
        public List<string> tags { get; set; }
        public int created_at { get; set; }
        public int released_at { get; set; }
    }

    public class SearchJson
    {
        public List<SA> hits { get; set; }
    }

    public class Hit
    {
        public double GetRating() => Math.Round((((float)likes) / ((float)likes + (float)dislikes) * 10));

        public int? id { get; set; }
        public string name { get; set; }
        public List<string> titles { get; set; }
        public string slug { get; set; }
        public string description { get; set; }
        public int? views { get; set; }
        public int? interests { get; set; }
        public string poster_url { get; set; }
        public string cover_url { get; set; }
        public string brand { get; set; }
        public int? brand_id { get; set; }
        public int? duration_in_ms { get; set; }
        public bool? is_censored { get; set; }
        public int? rating { get; set; }
        public int? likes { get; set; }
        public int? dislikes { get; set; }
        public int? downloads { get; set; }
        public int? monthly_rank { get; set; }
        public List<string> tags
        {
            get;
            set;
        }

        public String tagsAsString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string a in tags)
                sb.Append($"{a}, ");
            return sb.ToString();
        }

        public int? created_at { get; set; }
        public int? released_at { get; set; }
    }

    public class SearchReq
    {
        public int? page { get; set; }
        public int? nbPages { get; set; }
        public int? nbHits { get; set; }
        public int? hitsPerPage { get; set; }
        public String hits
        {
            set
            {
                actualHits = JsonSerializer.Deserialize<List<Hit>>(value);
            }
        }

        public List<Hit> actualHits;
    }

    public class Slug
    {
        public int id { get; set; }
        public string slug { get; set; }
        public int anime_id { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
    }

    public class Episode
    {
        public int id { get; set; }
        public int number { get; set; }
#nullable enable
        public string? source { get; set; }
#nullable disable
        public int anime_id { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
    }

    public class TwistMoeAnimeInfo
    {
        public int id { get; set; }
        public string title { get; set; }
        public object alt_title { get; set; }
        public int season { get; set; }
        public int ongoing { get; set; }
        public string description { get; set; }
        public int hb_id { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public int hidden { get; set; }
        public object mal_id { get; set; }
        public Slug slug { get; set; }
        public List<Episode> episodes { get; set; }
    }
}
