export type MenuItem = {
  label: string
  to: string
  authOnly?: boolean
  guestOnly?: boolean
  disabled?: boolean
  children?: MenuItem[]
}
