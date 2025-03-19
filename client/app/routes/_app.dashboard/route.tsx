import { Link } from "@remix-run/react";

export const handle = {
    breadcrumb: () => <Link to="/dashboard">Dashboard</Link>,
};

export default function DashboardRoute() {
    return (
        <div className="text-center w-full bg-base-300 p-6">
            {/* Stats Cards */}
            <div className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
                <div className="stat bg-base-100 shadow rounded-box">
                    <div className="stat-figure text-primary">
                        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" className="inline-block w-8 h-8 stroke-current"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>
                    </div>
                    <div className="stat-title">Total Projects</div>
                    <div className="stat-value text-primary">89</div>
                </div>

                <div className="stat bg-base-100 shadow rounded-box">
                    <div className="stat-figure text-secondary">
                        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" className="inline-block w-8 h-8 stroke-current"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 6V4m0 2a2 2 0 100 4m0-4a2 2 0 110 4m-6 8a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4m6 6v10m6-2a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4"></path></svg>
                    </div>
                    <div className="stat-title">Total Tickets</div>
                    <div className="stat-value text-secondary">42</div>
                </div>

                <div className="stat bg-base-100 shadow rounded-box">
                    <div className="stat-figure text-accent">
                        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" className="inline-block w-8 h-8 stroke-current"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M5 8h14M5 8a2 2 0 110-4h14a2 2 0 110 4M5 8v10a2 2 0 002 2h10a2 2 0 002-2V8m-9 4h4"></path></svg>
                    </div>
                    <div className="stat-title">Open Tickets</div>
                    <div className="stat-value text-accent">128</div>
                </div>

                <div className="stat bg-base-100 shadow rounded-box">
                    <div className="stat-figure text-content">
                        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" className="inline-block w-8 h-8 stroke-current"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M5 8h14M5 8a2 2 0 110-4h14a2 2 0 110 4M5 8v10a2 2 0 002 2h10a2 2 0 002-2V8m-9 4h4"></path></svg>
                    </div>
                    <div className="stat-title">Closed Tickets</div>
                    <div className="stat-value text-content">128</div>
                </div>
            </div>


            {/* Recent Activity Table */}
            <div className="bg-base-100 rounded-box shadow mb-8">
                <h2 className="text-xl font-semibold p-6 pb-2">Recent Activity</h2>
                <div className="overflow-x-auto">
                    <table className="table table-zebra w-full 2xlxl:text-lg">
                        <thead>
                            <tr>
                                <th>Title</th>
                                <th>Status</th>
                                <th>Type</th>
                                <th>Priority</th>
                                <th>Assigned To</th>
                                <th>Last Updated</th>
                            </tr>
                        </thead>
                        <tbody>
                            {[...Array(5)].map((_, i) => (
                                <tr key={i}>
                                    <td className="font-medium">{["Login page crashes on mobile", "Cannot upload images", "Search results not filtering correctly", "Payment processing timeout", "User profile not saving changes"][i]}</td>
                                    <td>
                                        <div className={`badge ${["badge-error", "badge-warning", "badge-info", "badge-success", "badge-warning"][i]} h-auto w-max`}>
                                            {["Critical", "In Progress", "Under Review", "Resolved", "In Progress"][i]}
                                        </div>
                                    </td>
                                    <td>
                                        <div className={`badge ${["badge-error", "badge-info", "badge-success", "badge-error", "badge-error"][i]} h-auto w-max`}>
                                            {["Defect", "New Development", "General Task", "Defect", "Defect"][i]}
                                        </div>
                                    </td>
                                    <td>
                                        <div className={`badge ${["badge-warning", "badge-info", "badge-info", "badge-error", "badge-success"][i]} h-auto w-max`}>
                                            {["High", "Medium", "Medium", "Urgent", "Low"][i]}
                                        </div>
                                    </td>
                                    <td>{["Sarah Kim", "Alex Johnson", "Miguel Rodriguez", "Priya Patel", "Jordan Taylor"][i]}</td>
                                    <td>{["2 hours ago", "Yesterday", "3 days ago", "1 week ago", "Just now"][i]}</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            </div>

            {/* Team Performance */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
                <div className="bg-base-100 p-6 rounded-box shadow">
                    <h2 className="text-xl font-semibold mb-4">Team Performance</h2>
                    <div className="space-y-4">
                        {["Frontend Team", "Backend Team", "QA Team", "DevOps"].map((team, i) => (
                            <div key={i}>
                                <div className="flex justify-between mb-1">
                                    <span>{team}</span>
                                    <span>{[78, 92, 65, 88][i]}%</span>
                                </div>
                                <progress
                                    className={`progress ${["progress-primary", "progress-success", "progress-warning", "progress-info"][i]} w-full`}
                                    value={[78, 92, 65, 88][i]}
                                    max="100"
                                ></progress>
                            </div>
                        ))}
                    </div>
                </div>

                <div className="bg-base-100 p-6 rounded-box shadow">
                    <h2 className="text-xl font-semibold mb-4">Upcoming Deadlines</h2>
                    <ul className="space-y-4">
                        {[...Array(4)].map((_, i) => (
                            <li key={i} className="flex items-center gap-4">
                                <div className={`badge badge-lg ${["badge-primary", "badge-secondary", "badge-accent", "badge-neutral"][i]}`}>
                                    {["Oct 15", "Oct 22", "Nov 5", "Nov 18"][i]}
                                </div>
                                <div>
                                    <h3 className="font-medium">{["User Authentication Refactor", "Mobile App v2.0 Release", "Payment Gateway Integration", "Year-End Security Audit"][i]}</h3>
                                    <p className="text-sm opacity-70">{["3 days remaining", "10 days remaining", "24 days remaining", "37 days remaining"][i]}</p>
                                </div>
                            </li>
                        ))}
                    </ul>
                </div>
            </div>

            {/* Chart Section */}
            <div className="bg-base-100 p-6 rounded-box shadow mb-8">
                <h2 className="text-xl font-semibold mb-4">Bug Resolution Trends</h2>
                <div className="h-64 w-full bg-base-200 flex items-center justify-center">
                    <p className="text-base-content/60">Chart Visualization Placeholder</p>
                </div>
            </div>

            {/* Bottom Cards */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
                {[...Array(3)].map((_, i) => (
                    <div key={i} className="card bg-base-100 shadow-xl">
                        <div className="card-body">
                            <h2 className="card-title">{["System Status", "Recent Notifications", "Quick Actions"][i]}</h2>
                            <p>{[
                                "All systems operational. Last incident: 15 days ago.",
                                "You have 8 unread notifications. 3 require immediate attention.",
                                "Access frequently used tools and actions from here."
                            ][i]}</p>
                            <div className="card-actions justify-end">
                                <button className="btn btn-primary btn-sm">View Details</button>
                            </div>
                        </div>
                    </div>
                ))}
            </div>
        </div>
    )
}
