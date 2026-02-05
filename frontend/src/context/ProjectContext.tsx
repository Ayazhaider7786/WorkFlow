import { createContext, useContext, useState, ReactNode } from 'react';
import type { Project } from '../types';

interface ProjectContextType {
  currentProject: Project | null;
  setCurrentProject: (project: Project | null) => void;
}

const ProjectContext = createContext<ProjectContextType | undefined>(undefined);

export function ProjectProvider({ children }: { children: ReactNode }) {
  const [currentProject, setCurrentProject] = useState<Project | null>(() => {
    const saved = localStorage.getItem('currentProject');
    return saved ? JSON.parse(saved) : null;
  });

  const handleSetProject = (project: Project | null) => {
    setCurrentProject(project);
    if (project) {
      localStorage.setItem('currentProject', JSON.stringify(project));
    } else {
      localStorage.removeItem('currentProject');
    }
  };

  return (
    <ProjectContext.Provider value={{ currentProject, setCurrentProject: handleSetProject }}>
      {children}
    </ProjectContext.Provider>
  );
}

export function useProject() {
  const context = useContext(ProjectContext);
  if (context === undefined) {
    throw new Error('useProject must be used within a ProjectProvider');
  }
  return context;
}
