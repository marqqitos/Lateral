/**
 * Test setup validation
 * This file ensures that our testing environment is properly configured
 */

describe('Test Setup', () => {
  it('should have Jest configured correctly', () => {
    expect(typeof jest).toBe('object')
    expect(typeof describe).toBe('function')
    expect(typeof test).toBe('function')
    expect(typeof expect).toBe('function')
  })

  it('should have environment variables configured', () => {
    expect(process.env.NEXT_PUBLIC_API_BASE_URL).toBe('http://localhost:5082')
  })

  it('should have fetch mocked globally', () => {
    expect(typeof global.fetch).toBe('function')
    expect(jest.isMockFunction(global.fetch)).toBe(true)
  })

  it('should have testing library DOM matchers available', () => {
    const div = document.createElement('div')
    div.textContent = 'Test content'
    document.body.appendChild(div)

    expect(div).toBeInTheDocument()
    expect(div).toHaveTextContent('Test content')

    document.body.removeChild(div)
  })
})
