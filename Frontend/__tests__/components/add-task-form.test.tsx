import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { AddTaskForm } from '@/components/add-task-form'

describe('AddTaskForm', () => {
  const mockOnAddTask = jest.fn()

  beforeEach(() => {
    mockOnAddTask.mockClear()
  })

  it('should render form elements correctly', () => {
    render(<AddTaskForm onAddTask={mockOnAddTask} />)

    expect(screen.getByText('Add New Task')).toBeInTheDocument()
    expect(screen.getByText('Create a new task to track')).toBeInTheDocument()
    expect(screen.getByLabelText('Task Title')).toBeInTheDocument()
    expect(screen.getByLabelText('Description')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /add task/i })).toBeInTheDocument()
  })

  it('should have correct initial state', () => {
    render(<AddTaskForm onAddTask={mockOnAddTask} />)

    const titleInput = screen.getByLabelText('Task Title')
    const descriptionInput = screen.getByLabelText('Description')
    const submitButton = screen.getByRole('button', { name: /add task/i })

    expect(titleInput).toHaveValue('')
    expect(descriptionInput).toHaveValue('')
    expect(submitButton).not.toBeDisabled()
  })

  it('should update input values when typed', async () => {
    const user = userEvent.setup()

    render(<AddTaskForm onAddTask={mockOnAddTask} />)

    const titleInput = screen.getByLabelText('Task Title')
    const descriptionInput = screen.getByLabelText('Description')

    await user.type(titleInput, 'New Task')
    await user.type(descriptionInput, 'Task description')

    expect(titleInput).toHaveValue('New Task')
    expect(descriptionInput).toHaveValue('Task description')
  })

  it('should show validation error for empty title', async () => {
    const user = userEvent.setup()

    render(<AddTaskForm onAddTask={mockOnAddTask} />)

    const submitButton = screen.getByRole('button', { name: /add task/i })
    await user.click(submitButton)

    expect(screen.getByText('Please enter a task title')).toBeInTheDocument()
    expect(mockOnAddTask).not.toHaveBeenCalled()
  })

  it('should show validation error for whitespace-only title', async () => {
    const user = userEvent.setup()

    render(<AddTaskForm onAddTask={mockOnAddTask} />)

    const titleInput = screen.getByLabelText('Task Title')
    const submitButton = screen.getByRole('button', { name: /add task/i })

    await user.type(titleInput, '   ')
    await user.click(submitButton)

    expect(screen.getByText('Please enter a task title')).toBeInTheDocument()
    expect(mockOnAddTask).not.toHaveBeenCalled()
  })

  it('should clear validation error when valid title is entered', async () => {
    const user = userEvent.setup()

    render(<AddTaskForm onAddTask={mockOnAddTask} />)

    const titleInput = screen.getByLabelText('Task Title')
    const submitButton = screen.getByRole('button', { name: /add task/i })

    // First trigger validation error
    await user.click(submitButton)
    expect(screen.getByText('Please enter a task title')).toBeInTheDocument()

    // Then type valid title
    await user.type(titleInput, 'Valid title')

    // Error should be cleared
    expect(screen.queryByText('Please enter a task title')).not.toBeInTheDocument()
  })

  it('should submit form with title only', async () => {
    const user = userEvent.setup()
    mockOnAddTask.mockResolvedValue({ success: true })

    render(<AddTaskForm onAddTask={mockOnAddTask} />)

    const titleInput = screen.getByLabelText('Task Title')
    const submitButton = screen.getByRole('button', { name: /add task/i })

    await user.type(titleInput, 'New Task')
    await user.click(submitButton)

    expect(mockOnAddTask).toHaveBeenCalledWith('New Task', '')
  })

  it('should submit form with title and description', async () => {
    const user = userEvent.setup()
    mockOnAddTask.mockResolvedValue({ success: true })

    render(<AddTaskForm onAddTask={mockOnAddTask} />)

    const titleInput = screen.getByLabelText('Task Title')
    const descriptionInput = screen.getByLabelText('Description')
    const submitButton = screen.getByRole('button', { name: /add task/i })

    await user.type(titleInput, 'New Task')
    await user.type(descriptionInput, 'Task description')
    await user.click(submitButton)

    expect(mockOnAddTask).toHaveBeenCalledWith('New Task', 'Task description')
  })

  it('should show loading state during submission', async () => {
    const user = userEvent.setup()
    let resolvePromise: (value: any) => void
    const submitPromise = new Promise(resolve => {
      resolvePromise = resolve
    })
    mockOnAddTask.mockReturnValue(submitPromise)

    render(<AddTaskForm onAddTask={mockOnAddTask} />)

    const titleInput = screen.getByLabelText('Task Title')
    const submitButton = screen.getByRole('button', { name: /add task/i })

    await user.type(titleInput, 'New Task')
    await user.click(submitButton)

    // Should show loading state
    expect(screen.getByText('Adding...')).toBeInTheDocument()
    expect(titleInput).toBeDisabled()
    expect(screen.getByLabelText('Description')).toBeDisabled()
    expect(submitButton).toBeDisabled()

    // Resolve the promise
    resolvePromise!({ success: true })
    await waitFor(() => {
      expect(screen.getByText('Add Task')).toBeInTheDocument()
    })
  })

  it('should clear form after successful submission', async () => {
    const user = userEvent.setup()
    mockOnAddTask.mockResolvedValue({ success: true })

    render(<AddTaskForm onAddTask={mockOnAddTask} />)

    const titleInput = screen.getByLabelText('Task Title')
    const descriptionInput = screen.getByLabelText('Description')
    const submitButton = screen.getByRole('button', { name: /add task/i })

    await user.type(titleInput, 'New Task')
    await user.type(descriptionInput, 'Task description')
    await user.click(submitButton)

    await waitFor(() => {
      expect(titleInput).toHaveValue('')
      expect(descriptionInput).toHaveValue('')
    })
  })

  it('should show error message for failed submission', async () => {
    const user = userEvent.setup()
    mockOnAddTask.mockResolvedValue({ success: false, error: 'Server error' })

    render(<AddTaskForm onAddTask={mockOnAddTask} />)

    const titleInput = screen.getByLabelText('Task Title')
    const submitButton = screen.getByRole('button', { name: /add task/i })

    await user.type(titleInput, 'New Task')
    await user.click(submitButton)

    await waitFor(() => {
      expect(screen.getByText('Server error')).toBeInTheDocument()
    })

    // Form should not be cleared on error
    expect(titleInput).toHaveValue('New Task')
  })

  it('should show default error message when no specific error provided', async () => {
    const user = userEvent.setup()
    mockOnAddTask.mockResolvedValue({ success: false })

    render(<AddTaskForm onAddTask={mockOnAddTask} />)

    const titleInput = screen.getByLabelText('Task Title')
    const submitButton = screen.getByRole('button', { name: /add task/i })

    await user.type(titleInput, 'New Task')
    await user.click(submitButton)

    await waitFor(() => {
      expect(screen.getByText('Failed to create task')).toBeInTheDocument()
    })
  })

  it('should handle unexpected errors during submission', async () => {
    const user = userEvent.setup()
    mockOnAddTask.mockRejectedValue(new Error('Network error'))

    render(<AddTaskForm onAddTask={mockOnAddTask} />)

    const titleInput = screen.getByLabelText('Task Title')
    const submitButton = screen.getByRole('button', { name: /add task/i })

    await user.type(titleInput, 'New Task')
    await user.click(submitButton)

    await waitFor(() => {
      expect(screen.getByText('An unexpected error occurred')).toBeInTheDocument()
    })
  })

  it('should handle form submission with Enter key', async () => {
    const user = userEvent.setup()
    mockOnAddTask.mockResolvedValue({ success: true })

    render(<AddTaskForm onAddTask={mockOnAddTask} />)

    const titleInput = screen.getByLabelText('Task Title')

    await user.type(titleInput, 'New Task')
    await user.keyboard('{Enter}')

    expect(mockOnAddTask).toHaveBeenCalledWith('New Task', '')
  })

  it('should have proper accessibility attributes', () => {
    render(<AddTaskForm onAddTask={mockOnAddTask} />)

    const titleInput = screen.getByLabelText('Task Title')
    const descriptionInput = screen.getByLabelText('Description')

    expect(titleInput).toHaveAttribute('id', 'title')
    expect(descriptionInput).toHaveAttribute('id', 'description')
    expect(titleInput).toHaveAttribute('placeholder', 'Enter task title...')
    expect(descriptionInput).toHaveAttribute('placeholder', 'Enter task description (optional)...')
  })
})
