import { Fragment, useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { Dialog, Transition } from '@headlessui/react';
import {
  Bars3Icon,
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
  ChevronLeftIcon,
  ChevronRightIcon,
  Cog6ToothIcon,
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
  { name: 'Settings', href: '/settings', icon: Cog6ToothIcon },
];

export default function Layout({ children }: { children: React.ReactNode }) {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [collapsed, setCollapsed] = useState(false);
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

  const SidebarContent = ({ onClose, isMobile = false }: { onClose?: () => void, isMobile?: boolean }) => (
    <div className={`flex grow flex-col overflow-y-auto bg-dark-900 ${collapsed && !isMobile ? 'px-2' : 'px-6'} pb-4 shadow-xl border-r border-dark-800/50 transition-all duration-300`}>
      {/* Logo */}
      <div className={`flex h-20 shrink-0 items-center ${collapsed && !isMobile ? 'justify-center' : 'gap-3'}`}>
        <div className="h-8 w-8 rounded-lg bg-gradient-to-br from-primary-400 to-primary-600 flex items-center justify-center shadow-lg shadow-primary-500/30 flex-shrink-0">
          <span className="text-white font-bold text-lg">W</span>
        </div>
        {(!collapsed || isMobile) && (
          <span className="text-xl font-bold bg-clip-text text-transparent bg-gradient-to-r from-white to-gray-400 transition-opacity duration-300">WorkFlow</span>
        )}
      </div>

      {/* Go to Projects Button */}
      <button
        onClick={() => {
          handleChangeProject();
          onClose?.();
        }}
        className={`group flex items-center gap-2 rounded-xl py-2 text-xs font-medium text-gray-400 hover:text-white hover:bg-white/5 transition-all duration-200 border border-transparent hover:border-white/5 ${collapsed && !isMobile ? 'justify-center px-0' : 'px-3'}`}
        title={collapsed && !isMobile ? "Go to Projects" : undefined}
      >
        <ArrowLeftIcon className="h-3.5 w-3.5 transition-transform group-hover:-translate-x-0.5 flex-shrink-0" />
        {(!collapsed || isMobile) && <span>Go to Projects</span>}
      </button>

      {/* Current Project Name */}
      {currentProject && (
        <div className={`mt-4 mb-2 bg-gradient-to-br from-dark-800 to-dark-900 rounded-xl border border-white/5 shadow-inner ${collapsed && !isMobile ? 'p-1 flex justify-center' : 'p-3'}`}>
          <div className={`flex items-center ${collapsed && !isMobile ? 'justify-center' : 'gap-3'}`}>
            <div className="p-1.5 rounded-lg bg-primary-500/10 text-primary-400 flex-shrink-0">
              <FolderIcon className="h-4 w-4" />
            </div>
            {(!collapsed || isMobile) && (
              <div className="min-w-0">
                <div className="text-sm font-semibold text-white truncate">{currentProject.name}</div>
                <div className="text-[10px] uppercase tracking-wider text-gray-500 font-medium">{currentProject.key}</div>
              </div>
            )}
          </div>
        </div>
      )}

      {/* Navigation */}
      <nav className="flex flex-1 flex-col mt-6">
        <ul className="flex flex-1 flex-col gap-y-1">
          <li>
            {(!collapsed || isMobile) && (
              <div className="text-xs font-semibold leading-6 text-gray-500 uppercase tracking-wider mb-2 px-2">Menu</div>
            )}
            <ul className="space-y-1">
              {navigation.map((item) => {
                const isActive = location.pathname === item.href;
                return (
                  <li key={item.name}>
                    <Link
                      to={item.href}
                      onClick={onClose}
                      className={`group flex items-center gap-x-3 rounded-xl py-2.5 text-sm font-medium leading-6 transition-all duration-200 ${isActive
                        ? 'bg-primary-500/10 text-primary-400 shadow-sm shadow-primary-500/5'
                        : 'text-gray-400 hover:bg-white/5 hover:text-white'
                        } ${collapsed && !isMobile ? 'justify-center px-0' : 'px-3'}`}
                      title={collapsed && !isMobile ? item.name : undefined}
                    >
                      <item.icon
                        className={`h-5 w-5 shrink-0 transition-colors ${isActive ? 'text-primary-400' : 'text-gray-500 group-hover:text-white'}`}
                      />
                      {(!collapsed || isMobile) && (
                        <>
                          {item.name}
                          {isActive && (
                            <div className="ml-auto w-1.5 h-1.5 rounded-full bg-primary-400 shadow-[0_0_8px_rgba(45,212,191,0.5)]" />
                          )}
                        </>
                      )}
                    </Link>
                  </li>
                );
              })}
            </ul>
          </li>

          {/* User info at bottom */}
          <li className="mt-auto">
            <div className="border-t border-white/10 pt-4 mt-4">
              <div className={`flex items-center gap-x-3 rounded-xl hover:bg-white/5 transition-colors cursor-pointer group ${collapsed && !isMobile ? 'justify-center p-2' : 'px-2 py-3'}`}>
                <div className="h-9 w-9 rounded-full bg-gradient-to-br from-primary-500 to-indigo-600 flex items-center justify-center shadow-lg ring-2 ring-dark-900 text-white font-medium text-sm flex-shrink-0">
                  {user?.firstName?.[0]}{user?.lastName?.[0]}
                </div>
                {(!collapsed || isMobile) && (
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-semibold text-white truncate group-hover:text-primary-200 transition-colors">
                      {user?.firstName} {user?.lastName}
                    </p>
                    <p className="text-xs text-gray-500 truncate">{user?.email}</p>
                  </div>
                )}
                {(!collapsed || isMobile) && (
                  <button
                    onClick={(e) => { e.stopPropagation(); handleLogout(); }}
                    className="p-1.5 text-gray-500 hover:text-white hover:bg-white/10 rounded-lg transition-all"
                    title="Sign out"
                  >
                    <ArrowRightOnRectangleIcon className="h-5 w-5" />
                  </button>
                )}
              </div>
            </div>
          </li>
        </ul>
      </nav>

      {/* Collapse Toggle (Desktop only) */}
      {!isMobile && (
        <button
          onClick={() => setCollapsed(!collapsed)}
          className="absolute -right-3 top-20 bg-dark-800 border border-dark-700 text-gray-400 hover:text-white rounded-full p-1 shadow-lg hover:bg-dark-700 transition-colors"
        >
          {collapsed ? (
            <ChevronRightIcon className="h-4 w-4" />
          ) : (
            <ChevronLeftIcon className="h-4 w-4" />
          )}
        </button>
      )}
    </div>
  );

  return (
    <div className="min-h-screen bg-[#F8FAFC]">
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
            <div className="fixed inset-0 bg-dark-900/80 backdrop-blur-sm" />
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
              <Dialog.Panel className="relative mr-16 flex w-full max-w-[280px] flex-1">
                <SidebarContent onClose={() => setSidebarOpen(false)} isMobile={true} />
              </Dialog.Panel>
            </Transition.Child>
          </div>
        </Dialog>
      </Transition.Root>

      {/* Desktop sidebar */}
      <div className={`hidden lg:fixed lg:inset-y-0 lg:z-50 lg:flex lg:flex-col transition-all duration-300 ${collapsed ? 'lg:w-[88px]' : 'lg:w-72'}`}>
        <SidebarContent />
      </div>

      {/* Mobile header */}
      <div className="sticky top-0 z-40 flex items-center gap-x-4 bg-dark-900 px-4 py-3 shadow-md lg:hidden">
        <button
          type="button"
          className="-m-2 p-2 text-gray-400 hover:text-white"
          onClick={() => setSidebarOpen(true)}
        >
          <Bars3Icon className="h-6 w-6" />
        </button>
        <div className="flex-1 text-sm font-semibold text-white">{currentProject?.name || 'WorkFlow'}</div>
      </div>

      {/* Main content */}
      <main className={`min-h-screen transition-all duration-300 ${collapsed ? 'lg:pl-[88px]' : 'lg:pl-72'}`}>
        <div className={`px-4 py-8 max-w-7xl mx-auto transition-all duration-300 ${collapsed ? 'lg:px-6' : 'sm:px-6 lg:px-8'}`}>
          {children}
        </div>
      </main>
    </div>
  );
}
