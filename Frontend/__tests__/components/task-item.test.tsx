import { render, screen, fireEvent } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { TaskItem } from '@/components/task-item'
import { Task } from '@/lib/api'

describe('TaskItem', () => {
  const mockOnToggleCompletion = jest.fn()
  const mockOnDeleteTask = jest.fn()

  const mockTask: Task = {
    id: 1,
    title: 'Test Task',
    description: 'Test Description',
    isCompleted: false,
    createdAt: '2023-01-01T00:00:00Z',
    updatedAt: '2023-01-01T00:00:00Z',
  }

  beforeEach(() => {
    mockOnToggleCompletion.mockClear()
    mockOnDeleteTask.mockClear()
  })

  it('should render task information correctly', () => {
    render(
      <TaskItem
        task={mockTask}
        onToggleCompletion={mockOnToggleCompletion}
        onDeleteTask={mockOnDeleteTask}
      />
    )

    expect(screen.getByText('Test Task')).toBeInTheDocument()
    expect(screen.getByText('Test Description')).toBeInTheDocument()
  })

  it('should render task without description', () => {
    const taskWithoutDescription: Task = {
      ...mockTask,
      description: undefined,
    }

    render(
      <TaskItem
        task={taskWithoutDescription}
        onToggleCompletion={mockOnToggleCompletion}
        onDeleteTask={mockOnDeleteTask}
      />
    )

    expect(screen.getByText('Test Task')).toBeInTheDocument()
    expect(screen.queryByText('Test Description')).not.toBeInTheDocument()
  })

  it('should show completed task styling', () => {
    const completedTask: Task = {
      ...mockTask,
      isCompleted: true,
    }

    render(
      <TaskItem
        task={completedTask}
        onToggleCompletion={mockOnToggleCompletion}
        onDeleteTask={mockOnDeleteTask}
      />
    )

    const title = screen.getByText('Test Task')
    expect(title).toHaveClass('text-gray-400', 'line-through')

    const description = screen.getByText('Test Description')
    expect(description).toHaveClass('text-gray-300')
  })

  it('should show active task styling', () => {
    render(
      <TaskItem
        task={mockTask}
        onToggleCompletion={mockOnToggleCompletion}
        onDeleteTask={mockOnDeleteTask}
      />
    )

    const title = screen.getByText('Test Task')
    expect(title).toHaveClass('text-gray-900')
    expect(title).not.toHaveClass('line-through')

    const description = screen.getByText('Test Description')
    expect(description).toHaveClass('text-gray-600')
  })

  it('should toggle completion when checkbox is clicked', async () => {
    const user = userEvent.setup()

    render(
      <TaskItem
        task={mockTask}
        onToggleCompletion={mockOnToggleCompletion}
        onDeleteTask={mockOnDeleteTask}
      />
    )

    const checkbox = screen.getByRole('checkbox')
    await user.click(checkbox)

    expect(mockOnToggleCompletion).toHaveBeenCalledWith(1)
    expect(mockOnToggleCompletion).toHaveBeenCalledTimes(1)
  })

  it('should toggle completion when task content is clicked', async () => {
    const user = userEvent.setup()

    render(
      <TaskItem
        task={mockTask}
        onToggleCompletion={mockOnToggleCompletion}
        onDeleteTask={mockOnDeleteTask}
      />
    )

    const taskContent = screen.getByText('Test Task')
    await user.click(taskContent)

    expect(mockOnToggleCompletion).toHaveBeenCalledWith(1)
    expect(mockOnToggleCompletion).toHaveBeenCalledTimes(1)
  })

  it('should delete task when delete button is clicked', async () => {
    const user = userEvent.setup()

    render(
      <TaskItem
        task={mockTask}
        onToggleCompletion={mockOnToggleCompletion}
        onDeleteTask={mockOnDeleteTask}
      />
    )

    const deleteButton = screen.getByLabelText('Delete task: Test Task')
    await user.click(deleteButton)

    expect(mockOnDeleteTask).toHaveBeenCalledWith(1)
    expect(mockOnDeleteTask).toHaveBeenCalledTimes(1)
  })

  it('should have correct checkbox state for completed task', () => {
    const completedTask: Task = {
      ...mockTask,
      isCompleted: true,
    }

    render(
      <TaskItem
        task={completedTask}
        onToggleCompletion={mockOnToggleCompletion}
        onDeleteTask={mockOnDeleteTask}
      />
    )

    const checkbox = screen.getByRole('checkbox')
    expect(checkbox).toBeChecked()
  })

  it('should have correct checkbox state for active task', () => {
    render(
      <TaskItem
        task={mockTask}
        onToggleCompletion={mockOnToggleCompletion}
        onDeleteTask={mockOnDeleteTask}
      />
    )

    const checkbox = screen.getByRole('checkbox')
    expect(checkbox).not.toBeChecked()
  })

  it('should have accessible delete button', () => {
    render(
      <TaskItem
        task={mockTask}
        onToggleCompletion={mockOnToggleCompletion}
        onDeleteTask={mockOnDeleteTask}
      />
    )

    const deleteButton = screen.getByLabelText('Delete task: Test Task')
    expect(deleteButton).toBeInTheDocument()
    expect(deleteButton).toHaveClass('hover:text-red-600')
  })

  it('should handle click events independently', async () => {
    const user = userEvent.setup()

    render(
      <TaskItem
        task={mockTask}
        onToggleCompletion={mockOnToggleCompletion}
        onDeleteTask={mockOnDeleteTask}
      />
    )

    // Click delete button should not trigger toggle
    const deleteButton = screen.getByLabelText('Delete task: Test Task')
    await user.click(deleteButton)

    expect(mockOnDeleteTask).toHaveBeenCalledTimes(1)
    expect(mockOnToggleCompletion).not.toHaveBeenCalled()

    mockOnDeleteTask.mockClear()

    // Click task content should trigger toggle
    const taskContent = screen.getByText('Test Task')
    await user.click(taskContent)

    expect(mockOnToggleCompletion).toHaveBeenCalledTimes(1)
    expect(mockOnDeleteTask).not.toHaveBeenCalled()
  })
})
