import { Link } from 'react-router-dom';

export default function About() {
  return (
    <div className="bg-white min-h-screen">
      {/* Header */}
      <header className="border-b border-gray-200">
        <nav className="flex items-center justify-between p-6 lg:px-8 max-w-7xl mx-auto">
          <div className="flex lg:flex-1">
            <Link to="/" className="text-2xl font-bold text-indigo-600">WorkFlow</Link>
          </div>
          <div className="hidden lg:flex lg:gap-x-8">
            <Link to="/" className="text-sm font-semibold text-gray-900 hover:text-indigo-600">Home</Link>
            <Link to="/about" className="text-sm font-semibold text-indigo-600">About Us</Link>
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

      {/* Content */}
      <main className="py-16">
        <div className="mx-auto max-w-3xl px-6 lg:px-8">
          <h1 className="text-4xl font-bold tracking-tight text-gray-900 mb-8">About Us</h1>
          
          <div className="prose prose-lg prose-indigo">
            <p className="text-lg text-gray-600 mb-6">
              WorkFlow is a comprehensive management and ticketing platform designed to help modern teams work more efficiently.
            </p>

            <h2 className="text-2xl font-bold text-gray-900 mt-12 mb-4">Our Mission</h2>
            <p className="text-gray-600 mb-6">
              We believe that great tools enable great work. Our mission is to provide teams with an intuitive, powerful platform that streamlines project management, enhances collaboration, and drives productivity.
            </p>

            <h2 className="text-2xl font-bold text-gray-900 mt-12 mb-4">Our Vision</h2>
            <p className="text-gray-600 mb-6">
              To become the go-to solution for companies seeking a unified platform for task management, document tracking, and team collaboration. We envision a world where teams spend less time managing work and more time doing meaningful work.
            </p>

            <h2 className="text-2xl font-bold text-gray-900 mt-12 mb-4">What We Offer</h2>
            <ul className="list-disc pl-6 text-gray-600 space-y-3 mb-6">
              <li><strong>Multi-tenant Architecture:</strong> Each company gets its own isolated workspace with complete data privacy.</li>
              <li><strong>Flexible Workflows:</strong> Customize your Kanban boards with statuses that match your process.</li>
              <li><strong>Sprint Management:</strong> Plan, execute, and track time-boxed iterations.</li>
              <li><strong>File Ticket Tracking:</strong> Monitor physical and digital documents through your organization.</li>
              <li><strong>Role-based Access:</strong> Control who can view, edit, and manage different aspects of your projects.</li>
              <li><strong>Activity Logging:</strong> Full audit trail of all changes for compliance and accountability.</li>
            </ul>

            <h2 className="text-2xl font-bold text-gray-900 mt-12 mb-4">Why Choose WorkFlow?</h2>
            <p className="text-gray-600 mb-6">
              Unlike complex enterprise tools that require extensive training, WorkFlow is designed with simplicity in mind. Get your team up and running in minutes, not days. Our clean interface focuses on what matters: getting work done.
            </p>

            <div className="mt-12 p-6 bg-indigo-50 rounded-lg">
              <h3 className="text-xl font-semibold text-indigo-900 mb-2">Ready to get started?</h3>
              <p className="text-indigo-700 mb-4">
                Register your company today and experience the difference.
              </p>
              <Link
                to="/register"
                className="inline-block rounded-md bg-indigo-600 px-4 py-2 text-sm font-semibold text-white shadow-sm hover:bg-indigo-500"
              >
                Register Now
              </Link>
            </div>
          </div>
        </div>
      </main>

      {/* Footer */}
      <footer className="bg-gray-900 py-12 mt-16">
        <div className="mx-auto max-w-7xl px-6 lg:px-8">
          <div className="flex flex-col md:flex-row justify-between items-center">
            <span className="text-xl font-bold text-white">WorkFlow</span>
            <div className="flex gap-x-6 mt-4 md:mt-0">
              <Link to="/about" className="text-sm text-gray-400 hover:text-white">About</Link>
              <Link to="/contact" className="text-sm text-gray-400 hover:text-white">Contact</Link>
              <Link to="/login" className="text-sm text-gray-400 hover:text-white">Login</Link>
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
