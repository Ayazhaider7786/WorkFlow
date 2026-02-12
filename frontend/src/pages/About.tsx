import { Link } from 'react-router-dom';
import { ArrowRightIcon, CheckCircleIcon, UserGroupIcon, ChartBarIcon, ShieldCheckIcon } from '@heroicons/react/24/outline';

export default function About() {
  return (
    <div className="min-h-screen bg-slate-900 text-white selection:bg-indigo-500 selection:text-white font-sans">

      {/* Navbar Overlay */}
      <header className="absolute top-0 left-0 right-0 z-50 pt-6 px-6 lg:px-8">
        <nav className="flex items-center justify-between max-w-7xl mx-auto rounded-2xl bg-white/5 backdrop-blur-lg border border-white/10 p-4">
          <div className="flex lg:flex-1">
            <Link to="/" className="text-2xl font-bold bg-clip-text text-transparent bg-gradient-to-r from-white to-indigo-300">
              WorkFlow
            </Link>
          </div>
          <div className="hidden lg:flex lg:gap-x-8">
            <Link to="/" className="text-sm font-medium text-gray-300 hover:text-white transition-colors">Home</Link>
            <Link to="/about" className="text-sm font-medium text-white">About Us</Link>
            <Link to="/contact" className="text-sm font-medium text-gray-300 hover:text-white transition-colors">Contact</Link>
          </div>
          <div className="flex flex-1 justify-end gap-x-4 items-center">
            <Link to="/login" className="text-sm font-medium text-gray-300 hover:text-white transition-colors">
              Login
            </Link>
            <Link to="/register" className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white shadow-lg shadow-indigo-500/30 hover:bg-indigo-500 transition-all">
              Get Started
            </Link>
          </div>
        </nav>
      </header>

      {/* Hero Section */}
      <div className="relative isolate overflow-hidden pt-36 pb-24 sm:pb-32">
        {/* Background Gradients */}
        <div className="absolute top-0 left-0 w-full h-full overflow-hidden -z-10">
          <div className="absolute top-[-10%] left-[20%] w-[40%] h-[40%] rounded-full bg-purple-600/20 blur-[120px]" />
          <div className="absolute top-[20%] right-[10%] w-[30%] h-[50%] rounded-full bg-indigo-600/20 blur-[120px]" />
        </div>

        <div className="mx-auto max-w-7xl px-6 lg:px-8 text-center relative z-10">
          <h1 className="text-4xl font-bold tracking-tight text-white sm:text-6xl mb-6 bg-clip-text text-transparent bg-gradient-to-br from-white via-gray-200 to-gray-400">
            Empowering Teams to <br className="hidden sm:block" /> Build the Future
          </h1>
          <p className="mt-6 text-lg leading-8 text-gray-400 max-w-2xl mx-auto">
            WorkFlow is the unified platform where modern teams plan, execute, and scale their most ambitious projects. clear, fast, and powerful.
          </p>
          <div className="mt-10 flex items-center justify-center gap-x-6">
            <Link to="/register" className="rounded-lg bg-white text-indigo-900 px-6 py-3 text-sm font-semibold shadow-xl hover:bg-gray-100 transition-all flex items-center gap-2">
              Start for free <ArrowRightIcon className="h-4 w-4" />
            </Link>
            <a href="#mission" className="text-sm font-semibold leading-6 text-white hover:text-gray-300 transition-colors">
              Learn more <span aria-hidden="true">→</span>
            </a>
          </div>
        </div>
      </div>

      {/* Mission & Vision Grid */}
      <div id="mission" className="py-24 sm:py-32 relative">
        <div className="mx-auto max-w-7xl px-6 lg:px-8">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-12">

            {/* Mission Card */}
            <div className="bg-white/5 backdrop-blur-sm border border-white/10 rounded-3xl p-8 hover:bg-white/10 transition-colors duration-500">
              <div className="h-12 w-12 rounded-lg bg-indigo-500/20 flex items-center justify-center mb-6">
                <ChartBarIcon className="h-6 w-6 text-indigo-400" />
              </div>
              <h2 className="text-2xl font-bold text-white mb-4">Our Mission</h2>
              <p className="text-gray-400 leading-relaxed">
                We believe that great tools enable great work. Our mission is to provide teams with an intuitive, powerful platform that streamlines project management, enhances collaboration, and drives productivity without the noise.
              </p>
            </div>

            {/* Vision Card */}
            <div className="bg-white/5 backdrop-blur-sm border border-white/10 rounded-3xl p-8 hover:bg-white/10 transition-colors duration-500">
              <div className="h-12 w-12 rounded-lg bg-purple-500/20 flex items-center justify-center mb-6">
                <UserGroupIcon className="h-6 w-6 text-purple-400" />
              </div>
              <h2 className="text-2xl font-bold text-white mb-4">Our Vision</h2>
              <p className="text-gray-400 leading-relaxed">
                To become the operating system for modern business. We envision a world where teams spend less time managing work and more time doing meaningful, creative work that moves the needle.
              </p>
            </div>

          </div>
        </div>
      </div>

      {/* Features List */}
      <div className="py-24 sm:py-32 bg-black/20">
        <div className="mx-auto max-w-7xl px-6 lg:px-8">
          <div className="mx-auto max-w-2xl text-center mb-16">
            <h2 className="text-base font-semibold leading-7 text-indigo-400">Everything you need</h2>
            <p className="mt-2 text-3xl font-bold tracking-tight text-white sm:text-4xl">
              Built for speed and scale
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
            {[
              { title: 'Multi-tenant Workspaces', desc: 'Secure, isolated environments for every team.' },
              { title: 'Flexible Workflows', desc: 'Customizable Kanban boards that fit your process.' },
              { title: 'Sprint Planning', desc: 'Manage iterations and backlogs with ease.' },
              { title: 'Document Tracking', desc: 'Keep all your file assets organized and linked.' },
              { title: 'Enterprise Security', desc: 'Role-based access control and audit logging.' },
              { title: 'Real-time Analytics', desc: 'Instant insights into team performance.' }
            ].map((feature, idx) => (
              <div key={idx} className="flex gap-4 p-4 rounded-xl hover:bg-white/5 transition-colors">
                <div className="flex-none">
                  <CheckCircleIcon className="h-6 w-6 text-indigo-500" />
                </div>
                <div>
                  <h3 className="font-semibold text-white">{feature.title}</h3>
                  <p className="mt-1 text-sm text-gray-400">{feature.desc}</p>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* CTA Section */}
      <div className="relative isolate py-24 px-6 sm:py-32 lg:px-8">
        <div className="mx-auto max-w-2xl text-center bg-gradient-to-b from-white/10 to-white/5 border border-white/10 rounded-3xl p-12 backdrop-blur-md">
          <h2 className="text-3xl font-bold tracking-tight text-white sm:text-4xl">
            Ready to transform your workflow?
          </h2>
          <p className="mx-auto mt-6 max-w-xl text-lg leading-8 text-gray-300">
            Join thousands of teams who have switched to a better way of working.
          </p>
          <div className="mt-10 flex items-center justify-center gap-x-6">
            <Link to="/register" className="rounded-md bg-indigo-600 px-3.5 py-2.5 text-sm font-semibold text-white shadow-sm hover:bg-indigo-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600">
              Get started today
            </Link>
          </div>
        </div>
      </div>

      {/* Footer */}
      <footer className="bg-black/40 border-t border-white/10 py-12">
        <div className="mx-auto max-w-7xl px-6 lg:px-8 flex flex-col md:flex-row justify-between items-center">
          <div className="flex items-center gap-2 mb-4 md:mb-0">
            <span className="text-xl font-bold text-white">WorkFlow</span>
          </div>
          <div className="flex gap-x-8 text-sm text-gray-400">
            <Link to="/about" className="hover:text-white transition-colors">About</Link>
            <Link to="/contact" className="hover:text-white transition-colors">Contact</Link>
            <Link to="/login" className="hover:text-white transition-colors">Login</Link>
          </div>
          <p className="mt-8 md:mt-0 text-xs text-gray-500">
            © {new Date().getFullYear()} WorkFlow. All rights reserved.
          </p>
        </div>
      </footer>
    </div>
  );
}
