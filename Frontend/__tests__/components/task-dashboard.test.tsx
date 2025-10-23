import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { TaskDashboard } from '@/components/task-dashboard'
import { Task } from '@/lib/api'

describe('TaskDashboard', () => {
  const mockOnToggleCompletion = jest.fn()
  const mockOnDeleteTask = jest.fn()

  const mockTasks: Task[] = [
    {
      id: 1,
      title: 'Active Task 1',
      description: 'Description 1',
      isCompleted: false,
      createdAt: '2023-01-01T00:00:00Z',
      updatedAt: '2023-01-01T00:00:00Z',
    },
    {
      id: 2,
      title: 'Active Task 2',
      description: 'Description 2',
      isCompleted: false,
      createdAt: '2023-01-02T00:00:00Z',
      updatedAt: '2023-01-02T00:00:00Z',
    },
    {
      id: 3,
      title: 'Completed Task 1',
      description: 'Description 3',
      isCompleted: true,
      createdAt: '2023-01-03T00:00:00Z',
      updatedAt: '2023-01-03T01:00:00Z',
    },
  ]

  beforeEach(() => {
    mockOnToggleCompletion.mockClear()
    mockOnDeleteTask.mockClear()
  })

  it('should render statistics cards correctly', () => {
    render(
      <TaskDashboard
        tasks={mockTasks}
        onToggleCompletion={mockOnToggleCompletion}
        onDeleteTask={mockOnDeleteTask}
      />
    )

    // Check total tasks
    expect(screen.getByText('3')).toBeInTheDocument()
    expect(screen.getByText('Total Tasks')).toBeInTheDocument()

    // Check active tasks
    expect(screen.getByText('2')).toBeInTheDocument()
    expect(screen.getByText('Active')).toBeInTheDocument()

    // Check completion percentage (1 completed out of 3 = 33%)
    expect(screen.getByText('33%')).toBeInTheDocument()
    expect(screen.getByText('Completed')).toBeInTheDocument()
  })

  it('should calculate statistics correctly with no tasks', () => {
    render(
      <TaskDashboard
        tasks={[]}
        onToggleCompletion={mockOnToggleCompletion}
        onDeleteTask={mockOnDeleteTask}
      />
    )

    // Use more specific queries to avoid multiple matches
    expect(screen.getByText('Total Tasks')).toBeInTheDocument()
    expect(screen.getByText('Active')).toBeInTheDocument()
    expect(screen.getByText('0%')).toBeInTheDocument() // Completion percentage
  })

  it('should calculate 100% completion correctly', () => {
    const allCompletedTasks: Task[] = [
      {
        id: 1,
        title: 'Completed Task 1',
        description: 'Description 1',
        isCompleted: true,
        createdAt: '2023-01-01T00:00:00Z',
        updatedAt: '2023-01-01T01:00:00Z',
      },
      {
        id: 2,
        title: 'Completed Task 2',
        description: 'Description 2',
        isCompleted: true,
        createdAt: '2023-01-02T00:00:00Z',
        updatedAt: '2023-01-02T01:00:00Z',
      },
    ]

    render(
      <TaskDashboard
        tasks={allCompletedTasks}
        onToggleCompletion={mockOnToggleCompletion}
        onDeleteTask={mockOnDeleteTask}
      />
    )

    expect(screen.getByText('100%')).toBeInTheDocument()
  })

  it('should render active tasks section', () => {
    render(
      <TaskDashboard
        tasks={mockTasks}
        onToggleCompletion={mockOnToggleCompletion}
        onDeleteTask={mockOnDeleteTask}
      />
    )

    expect(screen.getByText('Active Tasks')).toBeInTheDocument()
    expect(screen.getByText('2 tasks remaining')).toBeInTheDocument()

    // Should show active tasks
    expect(screen.getByText('Active Task 1')).toBeInTheDocument()
    expect(screen.getByText('Active Task 2')).toBeInTheDocument()
  })

  it('should render completed tasks section when tasks exist', () => {
    render(
      <TaskDashboard
        tasks={mockTasks}
        onToggleCompletion={mockOnToggleCompletion}
        onDeleteTask={mockOnDeleteTask}
      />
    )

    expect(screen.getByText('Completed Tasks')).toBeInTheDocument()
    expect(screen.getByText('1 task completed')).toBeInTheDocument()

    // Should show completed task
    expect(screen.getByText('Completed Task 1')).toBeInTheDocument()
  })

  it('should not render completed tasks section when no completed tasks exist', () => {
    const activeTasks = mockTasks.filter(task => !task.isCompleted)

    render(
      <TaskDashboard
        tasks={activeTasks}
        onToggleCompletion={mockOnToggleCompletion}
        onDeleteTask={mockOnDeleteTask}
      />
    )

    expect(screen.queryByText('Completed Tasks')).not.toBeInTheDocument()
  })

  it('should show empty state message when no active tasks', () => {
    const completedTasks = mockTasks.filter(task => task.isCompleted)

    render(
      <TaskDashboard
        tasks={completedTasks}
        onToggleCompletion={mockOnToggleCompletion}
        onDeleteTask={mockOnDeleteTask}
      />
    )

    expect(screen.getByText('No active tasks. Great job!')).toBeInTheDocument()
  })

  it('should use singular form for task counts when appropriate', () => {
    const singleTask: Task[] = [
      {
        id: 1,
        title: 'Single Task',
        description: 'Description',
        isCompleted: false,
        createdAt: '2023-01-01T00:00:00Z',
        updatedAt: '2023-01-01T00:00:00Z',
      },
    ]

    render(
      <TaskDashboard
        tasks={singleTask}
        onToggleCompletion={mockOnToggleCompletion}
        onDeleteTask={mockOnDeleteTask}
      />
    )

    expect(screen.getByText('1 task remaining')).toBeInTheDocument()
  })

  it('should use singular form for completed task count', () => {
    const singleCompletedTask: Task[] = [
      {
        id: 1,
        title: 'Completed Task',
        description: 'Description',
        isCompleted: true,
        createdAt: '2023-01-01T00:00:00Z',
        updatedAt: '2023-01-01T01:00:00Z',
      },
    ]

    render(
      <TaskDashboard
        tasks={singleCompletedTask}
        onToggleCompletion={mockOnToggleCompletion}
        onDeleteTask={mockOnDeleteTask}
      />
    )

    expect(screen.getByText('1 task completed')).toBeInTheDocument()
  })

  it('should pass callbacks to TaskItem components', async () => {
    const user = userEvent.setup()

    render(
      <TaskDashboard
        tasks={mockTasks}
        onToggleCompletion={mockOnToggleCompletion}
        onDeleteTask={mockOnDeleteTask}
      />
    )

    // Find the first task's checkbox and click it
    const checkboxes = screen.getAllByRole('checkbox')
    await user.click(checkboxes[0])

    expect(mockOnToggleCompletion).toHaveBeenCalledWith(1)
  })

  it('should render tasks in correct sections based on completion status', () => {
    render(
      <TaskDashboard
        tasks={mockTasks}
        onToggleCompletion={mockOnToggleCompletion}
        onDeleteTask={mockOnDeleteTask}
      />
    )

    // Verify active tasks are displayed
    expect(screen.getByText('Active Task 1')).toBeInTheDocument()
    expect(screen.getByText('Active Task 2')).toBeInTheDocument()

    // Verify completed task is displayed
    expect(screen.getByText('Completed Task 1')).toBeInTheDocument()

    // Verify sections exist
    expect(screen.getByText('Active Tasks')).toBeInTheDocument()
    expect(screen.getByText('Completed Tasks')).toBeInTheDocument()
  })

  it('should round completion percentage correctly', () => {
    // 1 completed out of 3 tasks = 33.333...% should round to 33%
    const tasksWithThird: Task[] = [
      ...mockTasks,
      {
        id: 4,
        title: 'Another Active Task',
        description: 'Description',
        isCompleted: false,
        createdAt: '2023-01-04T00:00:00Z',
        updatedAt: '2023-01-04T00:00:00Z',
      },
    ]

    render(
      <TaskDashboard
        tasks={tasksWithThird}
        onToggleCompletion={mockOnToggleCompletion}
        onDeleteTask={mockOnDeleteTask}
      />
    )

    // 1 completed out of 4 tasks = 25%
    expect(screen.getByText('25%')).toBeInTheDocument()
  })

  it('should have proper card styling and structure', () => {
    render(
      <TaskDashboard
        tasks={mockTasks}
        onToggleCompletion={mockOnToggleCompletion}
        onDeleteTask={mockOnDeleteTask}
      />
    )

    // Check for card elements
    const cards = screen.getAllByRole('generic').filter(el =>
      el.className.includes('bg-white')
    )

    expect(cards.length).toBeGreaterThan(0)
  })
})
