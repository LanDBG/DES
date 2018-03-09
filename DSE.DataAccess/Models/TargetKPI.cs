using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSE.DataAccess.Models
{
    public class TargetKPI
    {
        public Guid o_target_kpi_id { get; set; }

        public DateTime? start_date { get; set; }

        public string o_lookup_start_date_id { get; set; }

        public DateTime? end_date { get; set; }

        public string o_lookup_end_date_id { get; set; }

        public string period { get; set; }

        public string archived_name { get; set; }

        public string site_name { get; set; }

        public string region { get; set; }

        public string office_type { get; set; }

        public string site_type { get; set; }

        public string company_managed { get; set; }

        public int? rsf { get; set; }

        public int? usf { get; set; }

        public int? size_capacity { get; set; }

        public int? bu_allocation { get; set; }

        public int? bu_remain { get; set; }

        public int? ob_head_count { get; set; }

        public int? other_head_count { get; set; }

        public int? no_of_desks { get; set; }

        public int? no_of_desks_vacant { get; set; }

        public string access_control { get; set; }

        public decimal? rent_per_month { get; set; }

        public decimal? wpr_cost_per_month { get; set; }

        public decimal? wpr_cost_per_desk { get; set; }

        public decimal? wpr_cost_per_ob_hc { get; set; }

        public decimal? wpr_cost_per_rsf_sqf { get; set; }

        public decimal? rent_per_sqf_per_month { get; set; }

        public int? rsf_per_desk { get; set; }

        public int? usf_per_desk { get; set; }

        public decimal? per_utilization { get; set; }

        public decimal? per_site_allocated { get; set; }

        public decimal? per_ob_hc_attendance { get; set; }

        public decimal? per_visitor_attendance { get; set; }

        public DateTime? modification_date { get; set; }

        public DateTime? create_date { get; set; }

        public static TargetKPI Builder(IDataRecord record)
        {
            var o_target_kpi_id = record.GetGuid(record.GetOrdinal("o_target_kpi_id"));
            var start_date = record.IsDBNull(record.GetOrdinal("start_date")) ? null : (DateTime?)record.GetDateTime(record.GetOrdinal("start_date"));
            var o_lookup_start_date_id = record.GetString(record.GetOrdinal("o_lookup_start_date_id"));
            var end_date = record.IsDBNull(record.GetOrdinal("end_date")) ? null : (DateTime?)record.GetDateTime(record.GetOrdinal("end_date"));
            var o_lookup_end_date_id = record.GetString(record.GetOrdinal("o_lookup_end_date_id"));
            var period = record.GetString(record.GetOrdinal("period"));
            var archived_name = record.GetString(record.GetOrdinal("archived_name"));
            var site_name = record.GetString(record.GetOrdinal("site_name"));
            var region = record.GetString(record.GetOrdinal("region"));
            var office_type = record.GetString(record.GetOrdinal("office_type"));
            var site_type = record.GetString(record.GetOrdinal("site_type"));
            var company_managed = record.GetString(record.GetOrdinal("company_managed"));
            var rsf = record.IsDBNull(record.GetOrdinal("rsf")) ? 0 : record.GetInt32(record.GetOrdinal("rsf"));
            var usf = record.IsDBNull(record.GetOrdinal("usf")) ? 0 : record.GetInt32(record.GetOrdinal("usf"));
            var size_capacity = record.IsDBNull(record.GetOrdinal("size_capacity")) ? 0 : record.GetInt32(record.GetOrdinal("size_capacity"));
            var bu_allocation = record.IsDBNull(record.GetOrdinal("bu_allocation")) ? 0 : record.GetInt32(record.GetOrdinal("bu_allocation"));
            var bu_remain = record.IsDBNull(record.GetOrdinal("bu_remain")) ? 0 : record.GetInt32(record.GetOrdinal("bu_remain"));
            var ob_head_count = record.IsDBNull(record.GetOrdinal("ob_head_count")) ? 0 : record.GetInt32(record.GetOrdinal("ob_head_count"));
            var other_head_count = record.IsDBNull(record.GetOrdinal("other_head_count")) ? 0 : record.GetInt32(record.GetOrdinal("other_head_count"));
            var no_of_desks = record.IsDBNull(record.GetOrdinal("no_of_desks")) ? 0 : record.GetInt32(record.GetOrdinal("no_of_desks"));
            var no_of_desks_vacant = record.IsDBNull(record.GetOrdinal("no_of_desks_vacant")) ? 0 : record.GetInt32(record.GetOrdinal("no_of_desks_vacant"));
            var access_control = record.IsDBNull(record.GetOrdinal("access_control")) ? string.Empty : record.GetString(record.GetOrdinal("access_control"));
            var rent_per_month = record.IsDBNull(record.GetOrdinal("rent_per_month")) ? 0 : record.GetInt32(record.GetOrdinal("rent_per_month"));
            var wpr_cost_per_month = record.IsDBNull(record.GetOrdinal("wpr_cost_per_month")) ? 0 : record.GetInt32(record.GetOrdinal("wpr_cost_per_month"));
            var wpr_cost_per_desk = record.IsDBNull(record.GetOrdinal("wpr_cost_per_desk")) ? 0 : record.GetInt32(record.GetOrdinal("wpr_cost_per_desk"));
            var wpr_cost_per_ob_hc = record.IsDBNull(record.GetOrdinal("wpr_cost_per_ob_hc")) ? 0 : record.GetInt32(record.GetOrdinal("wpr_cost_per_ob_hc"));
            var wpr_cost_per_rsf_sqf = record.IsDBNull(record.GetOrdinal("wpr_cost_per_rsf_sqf")) ? 0 : record.GetInt32(record.GetOrdinal("wpr_cost_per_rsf_sqf"));
            var rent_per_sqf_per_month = record.IsDBNull(record.GetOrdinal("rent_per_sqf_per_month")) ? 0 : record.GetInt32(record.GetOrdinal("rent_per_sqf_per_month"));
            var rsf_per_desk = record.IsDBNull(record.GetOrdinal("rsf_per_desk")) ? 0 : record.GetInt32(record.GetOrdinal("rsf_per_desk"));
            var usf_per_desk = record.IsDBNull(record.GetOrdinal("usf_per_desk")) ? 0 : record.GetInt32(record.GetOrdinal("usf_per_desk"));
            var per_utilization = record.IsDBNull(record.GetOrdinal("per_utilization")) ? 0 : record.GetInt32(record.GetOrdinal("per_utilization"));
            var per_site_allocated = record.IsDBNull(record.GetOrdinal("per_site_allocated")) ? 0 : record.GetInt32(record.GetOrdinal("per_site_allocated"));
            var per_ob_hc_attendance = record.IsDBNull(record.GetOrdinal("per_ob_hc_attendance")) ? 0 : record.GetInt32(record.GetOrdinal("per_ob_hc_attendance"));
            var per_visitor_attendance = record.IsDBNull(record.GetOrdinal("per_visitor_attendance")) ? 0 : record.GetDecimal(record.GetOrdinal("per_visitor_attendance"));
            var modification_date = record.IsDBNull(record.GetOrdinal("modification_date")) ? null : (DateTime?)record.GetDateTime(record.GetOrdinal("modification_date"));
            var create_date = record.IsDBNull(record.GetOrdinal("create_date")) ? null : (DateTime?)record.GetDateTime(record.GetOrdinal("create_date"));

            return new TargetKPI
            {
                o_target_kpi_id = o_target_kpi_id,
                start_date = start_date,
                o_lookup_start_date_id = o_lookup_start_date_id,
                end_date = end_date,
                o_lookup_end_date_id = o_lookup_end_date_id,
                period = period,
                archived_name = archived_name,
                site_name = site_name,
                region = region,
                office_type = office_type,
                site_type = site_type,
                company_managed = company_managed,
                rsf = rsf,
                usf = usf,
                size_capacity = size_capacity,
                bu_allocation = bu_allocation,
                bu_remain = bu_remain,
                ob_head_count = ob_head_count,
                other_head_count = other_head_count,
                no_of_desks = no_of_desks,
                no_of_desks_vacant = no_of_desks_vacant,
                access_control = access_control,
                rent_per_month = rent_per_month,
                wpr_cost_per_month = wpr_cost_per_month,
                wpr_cost_per_desk = wpr_cost_per_desk,
                wpr_cost_per_ob_hc = wpr_cost_per_ob_hc,
                wpr_cost_per_rsf_sqf = wpr_cost_per_rsf_sqf,
                rent_per_sqf_per_month = rent_per_sqf_per_month,
                rsf_per_desk = rsf_per_desk,
                usf_per_desk = usf_per_desk,
                per_utilization = per_utilization,
                per_site_allocated = per_site_allocated,
                per_ob_hc_attendance = per_ob_hc_attendance,
                per_visitor_attendance = per_visitor_attendance,
                modification_date = modification_date,
                create_date = create_date
            };
        }

    }

}
