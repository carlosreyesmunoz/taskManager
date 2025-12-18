import { useState, useEffect } from 'react';
import { Task } from '../types';
import { taskService } from '../services/taskService';

interface TaskListProps {
  organizationId: number;
}

export const TaskList = ({ organizationId }: TaskListProps) => {
  const [tasks, setTasks] = useState<Task[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchTasks = async () => {
      try {
        setLoading(true);
        const data = await taskService.getTasks(organizationId);
        setTasks(data);
        setError(null);
      } catch (err) {
        setError('Failed to load tasks');
        console.error('Error fetching tasks:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchTasks();
  }, [organizationId]);

  const handleCompleteTask = async (taskId: number) => {
    try {
      await taskService.completeTask(taskId);
      setTasks(tasks.map(task => 
        task.id === taskId ? { ...task, status: 'completed' as const } : task
      ));
    } catch (err) {
      console.error('Error completing task:', err);
      alert('Failed to complete task');
    }
  };

  if (loading) {
    return <div className="loading">Loading tasks...</div>;
  }

  if (error) {
    return <div className="error">{error}</div>;
  }

  return (
    <div className="task-list">
      <h2>Tasks</h2>
      {tasks.length === 0 ? (
        <p>No tasks available</p>
      ) : (
        <ul>
          {tasks.map(task => (
            <li key={task.id} className={`task-item status-${task.status}`}>
              <div className="task-header">
                <h3>{task.title}</h3>
                <span className="task-points">{task.points} points</span>
              </div>
              {task.description && (
                <p className="task-description">{task.description}</p>
              )}
              <div className="task-footer">
                <span className="task-status">{task.status}</span>
                {task.status === 'uncompleted' && (
                  <button onClick={() => handleCompleteTask(task.id)}>
                    Mark Complete
                  </button>
                )}
              </div>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
};
