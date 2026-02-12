import { Link } from 'react-router-dom';
import {
  ViewColumnsIcon,
  DocumentDuplicateIcon,
  UsersIcon,
  ChartBarIcon,
  ClockIcon,
  ShieldCheckIcon,
  ArrowRightIcon
} from '@heroicons/react/24/outline';

const features = [
  {
    name: 'Kanban Board',
    description: 'Visualize your workflow with drag-and-drop task management that feels natural and intuitive.',
    icon: ViewColumnsIcon,
  },
  {
    name: 'Sprint Planning',
    description: 'Plan and execute time-boxed sprints for agile development with powerful velocity tracking.',
    icon: ClockIcon,
  },
  {
    name: 'File Tracking',
    description: 'Securely track physical and digital document transfers with a complete, immutable audit trail.',
    icon: DocumentDuplicateIcon,
  },
  {
    name: 'Team Collaboration',
    description: 'Seamlessly manage team members with granular role-based access control and permissions.',
    icon: UsersIcon,
  },
  {
    name: 'Analytics Dashboard',
    description: 'Gain actionable insights into team performance, project progress, and bottleneck identification.',
    icon: ChartBarIcon,
  },
  {
    name: 'Enterprise Security',
    description: 'Bank-grade data isolation between companies ensuring your sensitive information stays private.',
    icon: ShieldCheckIcon,
  },
];

export default function Home() {
  return (
    <div className="min-h-screen bg-slate-900 text-white selection:bg-indigo-500 selection:text-white font-sans">
      {/* Navbar Overlay */}
      <header className="absolute top-0 left-0 right-0 z-50 pt-6 px-6 lg:px-8">
        <nav className="flex items-center justify-between max-w-7xl mx-auto rounded-2xl bg-white/5 backdrop-blur-lg border border-white/10 p-4">
          <div className="flex lg:flex-1 items-center gap-2">
            <Link to="/" className="text-2xl font-bold bg-clip-text text-transparent bg-gradient-to-r from-white to-indigo-300">
              WorkFlow
            </Link>
          </div>
          <div className="hidden lg:flex lg:gap-x-8">
            <Link to="/" className="text-sm font-medium text-white">Home</Link>
            <Link to="/about" className="text-sm font-medium text-gray-300 hover:text-white transition-colors">About Us</Link>
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
      <div className="relative isolate pt-14 overflow-hidden">
        {/* Background Gradients */}
        <div className="absolute top-0 left-0 w-full h-full overflow-hidden pointer-events-none -z-10">
          <div className="absolute top-[-10%] left-[20%] w-[40%] h-[40%] rounded-full bg-purple-600/20 blur-[120px]" />
          <div className="absolute top-[20%] right-[10%] w-[30%] h-[50%] rounded-full bg-indigo-600/20 blur-[120px]" />
          <div className="absolute bottom-[0%] left-[30%] w-[50%] h-[30%] rounded-full bg-pink-600/10 blur-[100px]" />
        </div>

        <div className="py-24 sm:py-32 lg:pb-40">
          <div className="mx-auto max-w-7xl px-6 lg:px-8">
            <div className="mx-auto max-w-3xl text-center">
              <div className="mb-8 flex justify-center">
                <div className="relative rounded-full px-3 py-1 text-sm leading-6 text-gray-400 ring-1 ring-white/10 hover:ring-white/20 transition-all bg-white/5">
                  Announcing our new V2.0 release. <a href="#" className="font-semibold text-indigo-400 hover:text-indigo-300"><span className="absolute inset-0" aria-hidden="true" />Read more <span aria-hidden="true">&rarr;</span></a>
                </div>
              </div>
              <h1 className="text-5xl font-bold tracking-tight text-white sm:text-7xl mb-8 bg-clip-text text-transparent bg-gradient-to-b from-white via-gray-200 to-gray-500">
                Orchestrate your team's work with elegance
              </h1>
              <p className="mt-6 text-lg leading-8 text-gray-400 mb-10 max-w-2xl mx-auto">
                A powerful, intuitive platform designed for modern teams. Track projects, manage tasks, and collaborate in real-time without the clutter.
              </p>
              <div className="mt-10 flex items-center justify-center gap-x-6">
                <Link
                  to="/register"
                  className="rounded-lg bg-indigo-600 px-8 py-3.5 text-sm font-semibold text-white shadow-xl shadow-indigo-500/20 hover:bg-indigo-500 hover:shadow-2xl hover:shadow-indigo-500/30 hover:-translate-y-1 transition-all duration-200"
                >
                  Start for free
                </Link>
                <Link to="/about" className="group text-sm font-semibold leading-6 text-white flex items-center gap-2 hover:text-gray-300 transition-colors">
                  Learn more <ArrowRightIcon className="h-4 w-4 group-hover:translate-x-1 transition-transform" />
                </Link>
              </div>
            </div>

            {/* Dashboard Preview Image */}
            <div className="mt-16 flow-root sm:mt-24">
              <div className="-m-2 rounded-xl bg-white/5 p-2 ring-1 ring-inset ring-white/10 lg:-m-4 lg:rounded-2xl lg:p-4 backdrop-blur-sm">
                <div className="rounded-md bg-slate-900 shadow-2xl ring-1 ring-white/10 overflow-hidden">
                  <div className="h-[400px] w-full bg-gradient-to-br from-slate-800 to-slate-900 flex items-center justify-center text-gray-400 relative overflow-hidden">
                    {/* Placeholder for actual dashboard screenshot */}
                    <div className="absolute inset-0 bg-grid-white/[0.02] bg-[length:20px_20px]" />
                    <div className="text-center relative z-10">
                      <ChartBarIcon className="h-24 w-24 mx-auto text-indigo-500/50 mb-4" />
                      <span className="text-lg font-medium text-gray-500">Interactive Dashboard Preview</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Features Section */}
      <div className="py-24 sm:py-32 bg-black/20 relative">
        <div className="mx-auto max-w-7xl px-6 lg:px-8">
          <div className="mx-auto max-w-2xl text-center">
            <h2 className="text-base font-semibold leading-7 text-indigo-400 uppercase tracking-wide">Faster Workflow</h2>
            <p className="mt-2 text-3xl font-bold tracking-tight text-white sm:text-4xl">
              Everything you need to ship projects
            </p>
            <p className="mt-6 text-lg leading-8 text-gray-400">
              Stop juggling multiple tools. WorkFlow brings your tasks, docs, and team together in one beautiful interface.
            </p>
          </div>
          <div className="mx-auto mt-16 max-w-7xl sm:mt-20 lg:mt-24">
            <dl className="grid grid-cols-1 gap-x-8 gap-y-10 lg:max-w-none lg:grid-cols-3 lg:gap-y-16">
              {features.map((feature) => (
                <div key={feature.name} className="relative group bg-white/5 rounded-3xl p-8 hover:bg-white/10 hover:shadow-xl hover:shadow-indigo-500/10 hover:-translate-y-1 transition-all duration-300 ring-1 ring-white/10 backdrop-blur-sm">
                  <dt className="text-base font-semibold leading-7 text-white">
                    <div className="absolute left-8 top-8 flex h-12 w-12 items-center justify-center rounded-xl bg-indigo-600/20 text-indigo-400 group-hover:text-white group-hover:bg-indigo-600 transition-all duration-300">
                      <feature.icon className="h-6 w-6" aria-hidden="true" />
                    </div>
                    <div className="ml-16 pl-2 pt-2 text-lg">
                      {feature.name}
                    </div>
                  </dt>
                  <dd className="mt-4 text-base leading-7 text-gray-400 pl-2">
                    {feature.description}
                  </dd>
                </div>
              ))}
            </dl>
          </div>
        </div>
      </div>

      {/* CTA Section */}
      <div className="relative isolate overflow-hidden">
        <div className="px-6 py-24 sm:px-6 sm:py-32 lg:px-8">
          <div className="mx-auto max-w-2xl text-center bg-white/5 border border-white/10 rounded-3xl p-12 backdrop-blur-md">
            <h2 className="text-3xl font-bold tracking-tight text-white sm:text-4xl">
              Ready to boost your productivity?
              <br />
              Start using WorkFlow today.
            </h2>
            <p className="mx-auto mt-6 max-w-xl text-lg leading-8 text-gray-300">
              Join thousands of teams who have transformed how they work. Simple pricing, powerful features.
            </p>
            <div className="mt-10 flex items-center justify-center gap-x-6">
              <Link
                to="/register"
                className="rounded-lg bg-indigo-600 px-8 py-3.5 text-sm font-semibold text-white shadow-sm hover:bg-indigo-500 hover:-translate-y-0.5 transition-all duration-200"
              >
                Get started for free
              </Link>
              <Link to="/contact" className="text-sm font-semibold leading-6 text-white group flex items-center gap-2 hover:text-gray-300">
                Contact sales <span aria-hidden="true" className="group-hover:translate-x-1 transition-transform">→</span>
              </Link>
            </div>
          </div>
        </div>
      </div>

      {/* Footer */}
      <footer className="bg-black/40 border-t border-white/10 py-12">
        <div className="mx-auto max-w-7xl overflow-hidden px-6 lg:px-8">
          <div className="flex justify-center space-x-10 mb-8">
            <Link to="/about" className="text-sm leading-6 text-gray-400 hover:text-white transition-colors">About</Link>
            <Link to="#" className="text-sm leading-6 text-gray-400 hover:text-white transition-colors">Blog</Link>
            <Link to="#" className="text-sm leading-6 text-gray-400 hover:text-white transition-colors">Jobs</Link>
            <Link to="#" className="text-sm leading-6 text-gray-400 hover:text-white transition-colors">Press</Link>
            <Link to="#" className="text-sm leading-6 text-gray-400 hover:text-white transition-colors">Privacy</Link>
            <Link to="/contact" className="text-sm leading-6 text-gray-400 hover:text-white transition-colors">Contact</Link>
          </div>
          <p className="mt-8 text-center text-xs leading-5 text-gray-500">
            &copy; {new Date().getFullYear()} WorkFlow, Inc. All rights reserved.
          </p>
        </div>
      </footer>
    </div>
  );
}
