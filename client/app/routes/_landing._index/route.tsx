import { MetaFunction } from "@remix-run/react";
import MainNavbar from "~/routes/_landing/MainNavbar";

export const meta: MetaFunction = () => {
    return [
        { title: "Zap - Bug Tracking Made Simple" },
        { name: "description", content: "A modern bug tracking system for development teams" },
    ];
};

export async function loader() {
    return null;
}

export default function Index() {
    return (
        <div className="min-h-screen flex flex-col">
            <div className="max-w-7xl mx-auto p-4 flex-1">
                <header className="py-12 md:py-20 text-center">
                    <h1 className="text-5xl md:text-6xl font-bold text-base-content mb-4">Welcome to <span className="text-primary">Zap!</span></h1>
                    <p className="text-xl md:text-2xl text-base-content/80 max-w-3xl mx-auto">Track, manage, and resolve bugs faster with our streamlined issue tracking system.</p>

                    <div className="mt-10 flex flex-col sm:flex-row gap-4 justify-center">
                        <a href="/register" className="btn btn-primary btn-lg">Get Started</a>
                        <a href="/login" className="btn btn-outline btn-lg">Sign In</a>
                    </div>
                </header>

                {/* Features Section */}
                <section className="py-12 md:py-16">
                    <h2 className="text-3xl md:text-4xl font-bold text-center mb-12">Why Choose Zap?</h2>
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
                        <div className="card bg-base-200 shadow-sm">
                            <div className="card-body items-center text-center">
                                <div className="text-4xl text-primary mb-4">📊</div>
                                <h3 className="card-title text-xl">Real-time Dashboard</h3>
                                <p>Get instant visibility into project status and team performance.</p>
                            </div>
                        </div>
                        <div className="card bg-base-200 shadow-sm">
                            <div className="card-body items-center text-center">
                                <div className="text-4xl text-primary mb-4">🔄</div>
                                <h3 className="card-title text-xl">Streamlined Workflow</h3>
                                <p>Efficiently manage tickets from creation to resolution.</p>
                            </div>
                        </div>
                        <div className="card bg-base-200 shadow-sm">
                            <div className="card-body items-center text-center">
                                <div className="text-4xl text-primary mb-4">👥</div>
                                <h3 className="card-title text-xl">Team Collaboration</h3>
                                <p>Assign tickets, track progress, and communicate effectively.</p>
                            </div>
                        </div>
                    </div>
                </section>

                {/* CTA Section */}
                <section className="py-12 md:py-16 text-center">
                    <div className="bg-primary/10 rounded-box p-8 md:p-12">
                        <h2 className="text-3xl font-bold mb-4">Ready to get started?</h2>
                        <p className="text-lg mb-6 max-w-2xl mx-auto">Join thousands of teams already using Zap to streamline their bug tracking process.</p>
                        <a href="/register" className="btn btn-primary btn-lg">Sign Up Now</a>
                    </div>
                </section>
            </div>

            {/* Footer */}
            <footer className="bg-base-200 mt-auto">
                <div className="max-w-7xl mx-auto p-6 text-center">
                    <p>© {new Date().getFullYear()} Zap Bug Tracker. All rights reserved.</p>
                </div>
            </footer>
        </div>
    );
}
