import { Fragment, useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { Dialog, Transition } from '@headlessui/react';
import {
  Bars3Icon,
  XMarkIcon,
  HomeIcon,
  ViewColumnsIcon,
  QueueListIcon,
  ClockIcon,
  DocumentDuplicateIcon,
  UsersIcon,
  ArrowRightOnRectangleIcon,
  FolderIcon,
  ChartBarIcon,
  ArrowLeftIcon,
} from '@heroicons/react/24/outline';
import { useAuth } from '../context/AuthContext';
import { useProject } from '../context/ProjectContext';

const navigation = [
  { name: 'Dashboard', href: '/dashboard', icon: HomeIcon },
  { name: 'Board', href: '/board', icon: ViewColumnsIcon },
  { name: 'Backlog', href: '/backlog', icon: QueueListIcon },
  { name: 'Sprints', href: '/sprints', icon: ClockIcon },
  { name: 'File Tickets', href: '/file-tickets', icon: DocumentDuplicateIcon },
  { name: 'Activity', href: '/activity', icon: ChartBarIcon },
  { name: 'Team', href: '/team', icon: UsersIcon },
];

export default function Layout({ children }: { children: React.ReactNode }) {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const { user, logout } = useAuth();
  const { currentProject, setCurrentProject } = useProject();
  const location = useLocation();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const handleChangeProject = () => {
    setCurrentProject(null);
    navigate('/projects');
  };

  const SidebarContent = ({ onClose }: { onClose?: () => void }) => (
    <div className="flex grow flex-col overflow-y-auto bg-indigo-600 px-4 pb-4">
      {/* Logo */}
      <div className="flex h-12 shrink-0 items-center">
        <span className="text-lg font-bold text-white">WorkFlow</span>
      </div>

      {/* Go to Projects Button */}
      <button
        onClick={() => {
          handleChangeProject();
          onClose?.();
        }}
        className="flex items-center gap-2 rounded-md px-2 py-1.5 text-xs font-medium text-indigo-200 hover:bg-indigo-700 hover:text-white mt-1"
      >
        <ArrowLeftIcon className="h-3.5 w-3.5" />
        Go to Projects
      </button>

      {/* Current Project Name */}
      {currentProject && (
        <div className="mt-2 px-2 py-1.5 bg-indigo-700/50 rounded-md">
          <div className="flex items-center gap-2">
            <FolderIcon className="h-4 w-4 text-indigo-300" />
            <span className="text-sm font-semibold text-white truncate">{currentProject.name}</span>
          </div>
          <span className="text-xs text-indigo-300 ml-6">{currentProject.key}</span>
        </div>
      )}

      {/* Navigation */}
      <nav className="flex flex-1 flex-col mt-3">
        <ul className="flex flex-1 flex-col">
          <li>
            <ul className="space-y-0.5">
              {navigation.map((item) => (
                <li key={item.name}>
                  <Link
                    to={item.href}
                    onClick={onClose}
                    className={`group flex gap-x-2 rounded-md px-2 py-1.5 text-sm font-medium leading-6 ${
                      location.pathname === item.href
                        ? 'bg-indigo-700 text-white'
                        : 'text-indigo-200 hover:bg-indigo-700 hover:text-white'
                    }`}
                  >
                    <item.icon className="h-5 w-5 shrink-0" />
                    {item.name}
                  </Link>
                </li>
              ))}
            </ul>
          </li>

          {/* User info at bottom */}
          <li className="mt-auto">
            <div className="border-t border-indigo-500 pt-3">
              <div className="flex items-center gap-x-2 px-2 py-2">
                <div className="h-8 w-8 rounded-full bg-indigo-500 flex items-center justify-center">
                  <span className="text-xs font-medium text-white">
                    {user?.firstName?.[0]}{user?.lastName?.[0]}
                  </span>
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium text-white truncate">
                    {user?.firstName} {user?.lastName}
                  </p>
                  <p className="text-xs text-indigo-200 truncate">{user?.email}</p>
                </div>
              </div>
              <button
                onClick={handleLogout}
                className="flex w-full items-center gap-x-2 rounded-md px-2 py-1.5 text-sm font-medium text-indigo-200 hover:bg-indigo-700 hover:text-white"
              >
                <ArrowRightOnRectangleIcon className="h-5 w-5" />
                Sign out
              </button>
            </div>
          </li>
        </ul>
      </nav>
    </div>
  );

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Mobile sidebar */}
      <Transition.Root show={sidebarOpen} as={Fragment}>
        <Dialog as="div" className="relative z-50 lg:hidden" onClose={setSidebarOpen}>
          <Transition.Child
            as={Fragment}
            enter="transition-opacity ease-linear duration-300"
            enterFrom="opacity-0"
            enterTo="opacity-100"
            leave="transition-opacity ease-linear duration-300"
            leaveFrom="opacity-100"
            leaveTo="opacity-0"
          >
            <div className="fixed inset-0 bg-gray-900/80" />
          </Transition.Child>

          <div className="fixed inset-0 flex">
            <Transition.Child
              as={Fragment}
              enter="transition ease-in-out duration-300 transform"
              enterFrom="-translate-x-full"
              enterTo="translate-x-0"
              leave="transition ease-in-out duration-300 transform"
              leaveFrom="translate-x-0"
              leaveTo="-translate-x-full"
            >
              <Dialog.Panel className="relative mr-16 flex w-full max-w-[200px] flex-1">
                <SidebarContent onClose={() => setSidebarOpen(false)} />
              </Dialog.Panel>
            </Transition.Child>
          </div>
        </Dialog>
      </Transition.Root>

      {/* Desktop sidebar - reduced width from w-64 to w-52 */}
      <div className="hidden lg:fixed lg:inset-y-0 lg:z-50 lg:flex lg:w-52 lg:flex-col">
        <SidebarContent />
      </div>

      {/* Mobile header */}
      <div className="sticky top-0 z-40 flex items-center gap-x-4 bg-indigo-600 px-4 py-3 shadow-sm lg:hidden">
        <button
          type="button"
          className="-m-2 p-2 text-indigo-200"
          onClick={() => setSidebarOpen(true)}
        >
          <Bars3Icon className="h-6 w-6" />
        </button>
        <div className="flex-1 text-sm font-semibold text-white">{currentProject?.name || 'WorkFlow'}</div>
      </div>

      {/* Main content - adjusted padding to match new sidebar width */}
      <main className="lg:pl-52">
        <div className="px-4 py-4 sm:px-6 lg:px-6">
          {children}
        </div>
      </main>
    </div>
  );
}
