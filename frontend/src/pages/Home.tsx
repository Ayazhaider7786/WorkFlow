import { Link } from 'react-router-dom';
import { 
  ViewColumnsIcon, 
  DocumentDuplicateIcon, 
  UsersIcon, 
  ChartBarIcon,
  ClockIcon,
  ShieldCheckIcon
} from '@heroicons/react/24/outline';

const features = [
  {
    name: 'Kanban Board',
    description: 'Visualize your workflow with drag-and-drop task management.',
    icon: ViewColumnsIcon,
  },
  {
    name: 'Sprint Planning',
    description: 'Plan and execute time-boxed sprints for agile development.',
    icon: ClockIcon,
  },
  {
    name: 'File Tracking',
    description: 'Track physical and digital document transfers with full audit trail.',
    icon: DocumentDuplicateIcon,
  },
  {
    name: 'Team Collaboration',
    description: 'Manage team members with role-based access control.',
    icon: UsersIcon,
  },
  {
    name: 'Analytics Dashboard',
    description: 'Get insights into team performance and project progress.',
    icon: ChartBarIcon,
  },
  {
    name: 'Multi-tenant Security',
    description: 'Complete data isolation between companies.',
    icon: ShieldCheckIcon,
  },
];

export default function Home() {
  return (
    <div className="bg-white">
      {/* Header */}
      <header className="absolute inset-x-0 top-0 z-50">
        <nav className="flex items-center justify-between p-6 lg:px-8 max-w-7xl mx-auto">
          <div className="flex lg:flex-1">
            <span className="text-2xl font-bold text-indigo-600">WorkFlow</span>
          </div>
          <div className="hidden lg:flex lg:gap-x-8">
            <Link to="/" className="text-sm font-semibold text-gray-900 hover:text-indigo-600">Home</Link>
            <Link to="/about" className="text-sm font-semibold text-gray-900 hover:text-indigo-600">About Us</Link>
            <Link to="/contact" className="text-sm font-semibold text-gray-900 hover:text-indigo-600">Contact</Link>
          </div>
          <div className="flex flex-1 justify-end gap-x-4">
            <Link to="/login" className="text-sm font-semibold text-gray-900 hover:text-indigo-600 py-2 px-3">
              Login
            </Link>
            <Link to="/register" className="rounded-md bg-indigo-600 px-4 py-2 text-sm font-semibold text-white shadow-sm hover:bg-indigo-500">
              Register Company
            </Link>
          </div>
        </nav>
      </header>

      {/* Hero Section */}
      <div className="relative isolate pt-14">
        <div className="absolute inset-x-0 -top-40 -z-10 transform-gpu overflow-hidden blur-3xl sm:-top-80">
          <div className="relative left-[calc(50%-11rem)] aspect-[1155/678] w-[36.125rem] -translate-x-1/2 rotate-[30deg] bg-gradient-to-tr from-indigo-200 to-indigo-400 opacity-30 sm:left-[calc(50%-30rem)] sm:w-[72.1875rem]"></div>
        </div>
        
        <div className="py-24 sm:py-32 lg:pb-40">
          <div className="mx-auto max-w-7xl px-6 lg:px-8">
            <div className="mx-auto max-w-2xl text-center">
              <h1 className="text-4xl font-bold tracking-tight text-gray-900 sm:text-6xl">
                Streamline Your Team's Workflow
              </h1>
              <p className="mt-6 text-lg leading-8 text-gray-600">
                A powerful management and ticketing platform that helps companies track work, manage teams, and deliver projects efficiently.
              </p>
              <div className="mt-10 flex items-center justify-center gap-x-6">
                <Link
                  to="/register"
                  className="rounded-md bg-indigo-600 px-5 py-3 text-sm font-semibold text-white shadow-sm hover:bg-indigo-500 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
                >
                  Get Started Free
                </Link>
                <Link to="/about" className="text-sm font-semibold leading-6 text-gray-900">
                  Learn more <span aria-hidden="true">→</span>
                </Link>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Features Section */}
      <div className="py-24 sm:py-32 bg-gray-50">
        <div className="mx-auto max-w-7xl px-6 lg:px-8">
          <div className="mx-auto max-w-2xl text-center">
            <h2 className="text-base font-semibold leading-7 text-indigo-600">Everything you need</h2>
            <p className="mt-2 text-3xl font-bold tracking-tight text-gray-900 sm:text-4xl">
              Powerful Features for Modern Teams
            </p>
          </div>
          <div className="mx-auto mt-16 max-w-5xl">
            <dl className="grid grid-cols-1 gap-x-8 gap-y-10 sm:grid-cols-2 lg:grid-cols-3">
              {features.map((feature) => (
                <div key={feature.name} className="bg-white rounded-xl p-6 shadow-sm border border-gray-100">
                  <dt className="flex items-center gap-x-3 text-base font-semibold leading-7 text-gray-900">
                    <feature.icon className="h-6 w-6 text-indigo-600" />
                    {feature.name}
                  </dt>
                  <dd className="mt-2 text-sm leading-6 text-gray-600">{feature.description}</dd>
                </div>
              ))}
            </dl>
          </div>
        </div>
      </div>

      {/* CTA Section */}
      <div className="bg-gray-50 py-16 border-t border-gray-100">
        <div className="mx-auto max-w-7xl px-6 lg:px-8 text-center">
          <h2 className="text-3xl font-bold tracking-tight text-gray-900 sm:text-4xl">
            Ready to boost your team's productivity?
          </h2>
          <p className="mt-4 text-lg text-gray-600">
            Start managing your projects more efficiently today.
          </p>
          <div className="mt-8">
            <Link
              to="/register"
              className="rounded-md bg-indigo-600 px-6 py-3 text-sm font-semibold text-white shadow-sm hover:bg-indigo-500"
            >
              Register Your Company
            </Link>
          </div>
        </div>
      </div>

      {/* Footer */}
      <footer className="bg-gray-50 py-12 border-t border-gray-200">
        <div className="mx-auto max-w-7xl px-6 lg:px-8">
          <div className="flex flex-col md:flex-row justify-between items-center">
            <span className="text-xl font-bold text-indigo-600">WorkFlow</span>
            <div className="flex gap-x-6 mt-4 md:mt-0">
              <Link to="/about" className="text-sm text-gray-600 hover:text-indigo-600">About</Link>
              <Link to="/contact" className="text-sm text-gray-600 hover:text-indigo-600">Contact</Link>
              <Link to="/login" className="text-sm text-gray-600 hover:text-indigo-600">Login</Link>
            </div>
          </div>
          <p className="mt-8 text-center text-xs text-gray-500">
            © {new Date().getFullYear()} WorkFlow. All rights reserved.
          </p>
        </div>
      </footer>
    </div>
  );
}
